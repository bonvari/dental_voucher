using System.Threading.Tasks;

namespace uPLibrary.Nfc
{
    // delegate for handling events tag related (detected and lost)
    public delegate void TagEventHandler(object sender, NfcTagEventArgs e);

    /// <summary>
    /// Interface for NFC reader
    /// </summary>
    public interface INfcReader
    {
        bool IsRunning { get; set; }

        /// <summary>
        /// Open NFC reader to detect tag
        /// </summary>
        /// <param name="nfcTagType"></param>
        Task Open(NfcTagType nfcTagType);

        /// <summary>
        /// Close NFC reader
        /// </summary>
        void Close();

        /// <summary>
        /// Write and read response to/from NFC reader
        /// </summary>
        /// <param name="data">Data to write to NFC reader</param>
        /// <returns>Response received from NFC reader</returns>
        Task<byte[]> WriteRead(byte[] data);

        /// <summary>
        /// Event for tag detected
        /// </summary>
        event TagEventHandler TagDetected;

        /// <summary>
        /// Event for tag lost
        /// </summary>
        event TagEventHandler TagLost;
    }
}
