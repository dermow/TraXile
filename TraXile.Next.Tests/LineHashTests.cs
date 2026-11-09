using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>
    /// The parser's resume point (tx_kvstore "last_hash") must be stable across processes and match what TraXile 2.x stored.
    /// </summary>
    public class LineHashTests
    {
        [Fact]
        public void Matches_the_hash_TraXile_2x_stored()
        {
            // Real Client.txt line and the last_hash TraXile 2.x (.NET Framework, 64-bit) stored for it
            const string line = "2026/11/07 20:46:04 1745922015 f9532a49 [INFO Client 41416] Closing game gracefully";
            Assert.Equal(245348400, TrX_LineHash.Compute(line));
        }

        [Fact]
        public void Is_a_fixed_function_not_the_randomized_string_hash()
        {
            Assert.Equal(unchecked(5381 + 5381 * 1566083941), TrX_LineHash.Compute(""));
        }

        [Fact]
        public void Stops_at_NUL_like_the_framework()
        {
            Assert.Equal(TrX_LineHash.Compute("abc"), TrX_LineHash.Compute("abc\0def"));
        }
    }
}
