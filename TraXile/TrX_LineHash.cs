namespace TraXile
{
    /// <summary>
    /// Stable hash of a Client.txt line, used as the resume point of the log parser (tx_kvstore "last_hash").
    /// </summary>
    /// <remarks>
    /// string.GetHashCode() must not be used for this: on .NET it is randomized per process, so a stored hash
    /// never matches again after a restart. This is the (non-randomized) string.GetHashCode() of the 64-bit
    /// .NET Framework 4.x that TraXile 2.x ran on, so the resume points stored by 2.x stay valid.
    /// </remarks>
    public static class TrX_LineHash
    {
        public static int Compute(string line)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;
                int i = 0;

                // The Framework walked the NUL-terminated buffer, so it stopped at the first '\0'
                while (i < line.Length && line[i] != '\0')
                {
                    hash1 = ((hash1 << 5) + hash1) ^ line[i];
                    i++;
                    if (i >= line.Length || line[i] == '\0')
                    {
                        break;
                    }
                    hash2 = ((hash2 << 5) + hash2) ^ line[i];
                    i++;
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
