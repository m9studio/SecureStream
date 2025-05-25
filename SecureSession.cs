using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Security;

namespace M9Studio.SecureStream
{
    public class SecureSession<TAddress>
    {
        private readonly ISecureTransportAdapter<TAddress> _adapter;
        private readonly TAddress _address;
        private byte[]? _initialBuffer;

        private X25519PrivateKeyParameters? _privateKey;
        private X25519PublicKeyParameters? _remotePublicKey;
        private AesGcm? _aesGcm;
        private bool _isHandshakeComplete = false;
        public TAddress RemoteAddress => _address;

        internal bool _IsLive = true;
        public bool IsLive => _IsLive;

        public SecureSession(ISecureTransportAdapter<TAddress> adapter, TAddress address, byte[]? initialBuffer = null)
        {
            _adapter = adapter;
            _address = address;
            _initialBuffer = initialBuffer;
            //Console.WriteLine($"[SecureSession] Created with remote address = {_address}");
        }

        internal void PerformHandshakeAsClient()
        {
            var secureRandom = new SecureRandom();
            _privateKey = new X25519PrivateKeyParameters(secureRandom);
            var publicKey = _privateKey.GeneratePublicKey().GetEncoded();

            _adapter.SendTo(publicKey, _address);
            var serverPublicKey = _adapter.ReceiveFrom(_address);

            _remotePublicKey = new X25519PublicKeyParameters(serverPublicKey, 0);
            EstablishSymmetricKeys();
            _isHandshakeComplete = true;
        }

        internal void PerformHandshakeAsServer(byte[] clientPublicKey)
        {
            _remotePublicKey = new X25519PublicKeyParameters(clientPublicKey, 0);
            var secureRandom = new SecureRandom();
            _privateKey = new X25519PrivateKeyParameters(secureRandom);
            var publicKey = _privateKey.GeneratePublicKey().GetEncoded();

            _adapter.SendTo(publicKey, _address);
            EstablishSymmetricKeys();
            _isHandshakeComplete = true;
        }

        private void EstablishSymmetricKeys()
        {
            var sharedSecret = new byte[32];
            _privateKey!.GenerateSecret(_remotePublicKey!, sharedSecret, 0);

            var hkdf = new Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator(new Sha256Digest());
            hkdf.Init(new Org.BouncyCastle.Crypto.Parameters.HkdfParameters(sharedSecret, null, null));

            byte[] key = new byte[32];
            hkdf.GenerateBytes(key, 0, key.Length);

            _aesGcm = new AesGcm(key);
        }

        public bool Send(byte[] data)
        {
            if (!IsLive) throw new InvalidOperationException("Session not live.");
            if (!_isHandshakeComplete) throw new InvalidOperationException("Handshake not complete.");

            var nonce = RandomNumberGenerator.GetBytes(12);
            var encrypted = new byte[data.Length];
            var tag = new byte[16];

            _aesGcm!.Encrypt(nonce, data, encrypted, tag);
            var payload = nonce.Concat(tag).Concat(encrypted).ToArray();

            //Console.WriteLine($"[{_address}] Send: {BitConverter.ToString(payload).Replace("-", "")}");
            return _adapter.SendTo(payload, _address);
        }

        public byte[] Receive()
        {
            if (!_isHandshakeComplete) throw new InvalidOperationException("Handshake not complete.");

            //Console.WriteLine($"[SecureSession] Receive() waiting from {_address}");

            byte[] packet;
            if (_initialBuffer != null)
            {
                packet = _initialBuffer;
                _initialBuffer = null;
            }
            else
            {
                packet = _adapter.ReceiveFrom(_address);
            }

            //Console.WriteLine($"[{_address}] Receive: {BitConverter.ToString(packet).Replace("-", "")}");
            return Decrypt(packet);
        }

        private byte[] Decrypt(byte[] packet)
        {
            var nonce = packet.AsSpan(0, 12).ToArray();
            var tag = packet.AsSpan(12, 16).ToArray();
            var ciphertext = packet.AsSpan(28).ToArray();

            var decrypted = new byte[ciphertext.Length];
            _aesGcm!.Decrypt(nonce, ciphertext, tag, decrypted);
            return decrypted;
        }

    }
}