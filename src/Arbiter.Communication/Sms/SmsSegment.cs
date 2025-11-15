// Ignore Spelling: Sms

using System.Buffers;

namespace Arbiter.Communication.Sms;

/// <summary>
/// Provides utilities for calculating SMS message segments based on character encoding.
/// </summary>
public static class SmsSegment
{
    // GSM-7 basic character set for O(1) lookup using SearchValues
    private static readonly SearchValues<char> Gsm7Characters = SearchValues.Create("@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà");

    // GSM-7 extended character set (these require escape character, counting as 2 characters)
    private static readonly SearchValues<char> Gsm7Extended = SearchValues.Create("^{}\\[~]|€");

    /// <summary>
    /// Calculates the number of SMS segments required to send the specified message.
    /// </summary>
    /// <param name="message">The message to calculate segments for.</param>
    /// <returns>
    /// The number of SMS segments required. Returns 0 if the message is null or empty.
    /// For GSM-7 encoding: 1 segment for up to 160 characters, then 153 characters per additional segment.
    /// For UCS-2/Unicode encoding: 1 segment for up to 70 characters, then 67 characters per additional segment.
    /// </returns>
    /// <remarks>
    /// Extended GSM-7 characters (^{}\\[~]|€) count as 2 characters each due to the required escape sequence.
    /// If any character outside the GSM-7 character set is found, UCS-2/Unicode encoding is used for the entire message.
    /// </remarks>
    public static int Calculate(ReadOnlySpan<char> message)
    {
        if (message.IsEmpty)
            return 0;

        int effectiveLength = CalculateLength(message, out bool isGsm7);

        // GSM-7 encoding; Single SMS: 160 chars, Concatenated SMS: 153 chars per segment
        // UCS-2/Unicode encoding; Single SMS: 70 chars, Concatenated SMS: 67 chars per segment
        (int single, double concatenated) = isGsm7 ? (160, 153) : (70, 67);

        if (effectiveLength <= single)
            return 1;

        return (int)Math.Ceiling(effectiveLength / concatenated);
    }

    /// <summary>
    /// Calculates the effective length of the message and determines the encoding type.
    /// </summary>
    /// <param name="text">The text to calculate the length for.</param>
    /// <param name="isGsm7">
    /// When this method returns, contains <c>true</c> if the message can be encoded using GSM-7;
    /// otherwise, <c>false</c> if UCS-2/Unicode encoding is required.
    /// </param>
    /// <returns>
    /// The effective character length of the message. For GSM-7 encoding, extended characters count as 2.
    /// For UCS-2/Unicode encoding, returns the actual string length.
    /// </returns>
    private static int CalculateLength(ReadOnlySpan<char> text, out bool isGsm7)
    {
        isGsm7 = true;
        int length = 0;

        foreach (char c in text)
        {
            if (Gsm7Characters.Contains(c))
            {
                length++;
            }
            else if (Gsm7Extended.Contains(c))
            {
                length += 2; // Extended characters require escape sequence
            }
            else
            {
                // Non-GSM7 character found, must use UCS-2 encoding
                isGsm7 = false;
                return text.Length;
            }
        }

        return length;
    }
}
