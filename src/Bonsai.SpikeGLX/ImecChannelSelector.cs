using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.SpikeGLX
{
    /// <summary>
    /// Represents an operator that automatically generates a an array of channel 
    /// indices for SpikeFetch or SpikeStream.
    /// </summary>
    [Obsolete("Replaced by channel range parsing in Fetch.")]
    [Description("Automatically generates an array of channel indices for " + 
        "SpikeFetch or SpikeStream.")]
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class ImecChannelSelector : Source<int[]>
    {
        /// <summary>
        /// Gets or sets the IP address of the SpikeGLX command server
        /// </summary>
        [Description("IP address of the SpikeGLX command server." +
            "\"localhost\" evaluates to 127.0.0.1.")]
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port of the SpikeGLX command server.
        /// </summary>
        [Description("Port of the SpikeGLX command server.")]
        public int Port { get; set; } = 4142;

        /// <summary>
        /// Gets or sets the channels, represented as a comma separated list 
        /// of ranges, e.g., "0:10,15:20". Ranges includes both ends.  
        /// </summary>
        [Description("Channels, represented as a comma separated list of " +
            "ranges, e.g., \"0:10,15:20\". Ranges include both ends.")]
        public string Channels { get; set; }

        /// <summary>
        /// Gets or sets the the channel to optionally use to filter 
        /// the resulting channel array to only those sampled at the 
        /// same time. 
        /// </summary>
        [Description("Optionally filter the channels output to " +
            "only those acquired simultaneously with a given channel.")]
        public int? MuxedWith { get; set; } = null;

        /// <summary>
        /// Gets or sets the IMEC probe number to produce the channel
        /// array for.
        /// </summary>
        [Description("The IMEC probe number to produce the channel array for.")]
        public int ProbeNumber { get; set; } = 0;        

        /// <summary>
        /// Converts a string representing a range of channels into an array of integers. 
        /// </summary>
        /// <param name="channels">The channels string to convert. This string is composed of 
        /// comma separated ranges of channels, e.g., "0:10,20:5:100". The (optional) middle number
        /// indicates the step size and the first and last numbers indicate the (inclusive) bounds.
        /// </param>
        /// <returns>The channels, represented as enumerable of integers.</returns>
        private static IEnumerable<int> ParseChannels(string channels)
        {
            // Parse the provided channels into a list.
            return channels.Split(',')      
                  .Select(x => x.Split(':'))                                         
                  .Select(p => new {                                            
                      First = int.Parse(p.First()), 
                      Last = int.Parse(p.Last()),
                      Step = (p.Length == 3)? int.Parse(p.Skip(1).First()) : 1 
                  })
                  .SelectMany(x => Enumerable.Range(x.First, x.Last - x.First + 1)
                    .Where((y, i) => i % x.Step == 0))
                  .OrderBy(z => z)
                  .Distinct();
        }

        /// <summary>
        /// Filters a list of channels down to only those that are sampled at the same time
        /// as a given channel.
        /// </summary>
        /// <param name="channels">The array of channels to filter.</param>
        /// <param name="muxChannel">The channel number to filter with.</param>
        /// <returns>The filtered array of channels.</returns>
        private IEnumerable<int> FilterChannelsByMux(IEnumerable<int> channels, int muxChannel)
        {
            using SpikeGLX connection = new(Host, Port);

            // Get Probe part number
            IEnumerable<string> probeParams = connection.GetGeomMap(ProbeNumber);

            foreach(var probe in probeParams)
            {
                Console.WriteLine(probe);
            }

            string probe_pn = probeParams.Where(x => x.StartsWith("head_partNumber"))
                .SelectMany(x => x.Split('='))
                .Last();

            // If the part number is recognized, perform the filtering
            if (!NeuropixelsMuxGroups.MuxTables.TryGetValue(probe_pn, out int[][] muxGroups))
            {
                Console.WriteLine(String.Format("{0} is not a recognized part number. " + 
                    "Filtering by mux groups cannot be performed.", probe_pn));
            }
            else
            {
                int[] muxGroup = muxGroups.Where(x => x.Contains(muxChannel)).First();
                channels = channels.Where(ch => muxGroup.Contains(ch));
            }
            return channels;
        }

        /// <summary>
        /// Generates an observanle sequence of a single array of integers, containing channel
        /// indicates for a SpikeFetch or SpikeStream marble.
        /// </summary>
        /// <returns>
        /// A sequence of a single array of integers representing channels for
        /// a SpikeFetch or SpikeStream marble.
        /// </returns>
        public override IObservable<int[]> Generate()
        {
            // Parse the provided channels into a list. 
            IEnumerable<int> channels = ParseChannels(Channels);

            // If indicated, filter the channels by mux groups
            if (MuxedWith is not null)
            {
                channels = FilterChannelsByMux(channels, (int)MuxedWith);
            }

            // Return the channels, formatted as an array
            return Observable.Return(channels.ToArray());
        }
    }

    /// <summary>
    /// Represents the constant mux tables for each Neuropixels probe type
    /// </summary>
    [Obsolete("Replaced by channel range parsing in Fetch.")]
    public static class NeuropixelsMuxGroups
    {
        // Part numbers of neuropixels probes
        private static readonly string NP1PartNumber1 = "NP1100";
        private static readonly string NP1PartNumber2 = "PRB_1_4_0480_1_C";
        private static readonly string NP2PartNumber = "NP2013";

        // Mux tables of neuropixels probes
        private static readonly int[][] NP1MuxTable = new int[][]
        {
            new []{ 0, 1, 24, 25, 48, 49, 72, 73, 96, 97, 120, 121, 144, 145, 168, 169, 192,
                193, 216, 217, 240, 241, 264, 265, 288, 289, 312, 313, 336, 337, 360, 361 },
            new []{ 2, 3, 26, 27, 50, 51, 74, 75, 98, 99, 122, 123, 146, 147, 170, 171, 194,
                195, 218, 219, 242, 243, 266, 267, 290, 291, 314, 315, 338, 339, 362, 363 },
            new []{ 4, 5, 28, 29, 52, 53, 76, 77, 100, 101, 124, 125, 148, 149, 172, 173, 196,
                197, 220, 221, 244, 245, 268, 269, 292, 293, 316, 317, 340, 341, 364, 365 },
            new []{ 6, 7, 30, 31, 54, 55, 78, 79, 102, 103, 126, 127, 150, 151, 174, 175, 198,
                199, 222, 223, 246, 247, 270, 271, 294, 295, 318, 319, 342, 343, 366, 367 },
            new []{ 8, 9, 32, 33, 56, 57, 80, 81, 104, 105, 128, 129, 152, 153, 176, 177, 200,
                201, 224, 225, 248, 249, 272, 273, 296, 297, 320, 321, 344, 345, 368, 369 },
            new []{ 10, 11, 34, 35, 58, 59, 82, 83, 106, 107, 130, 131, 154, 155, 178, 179, 202,
                203, 226, 227, 250, 251, 274, 275, 298, 299, 322, 323, 346, 347, 370, 371 },
            new []{ 12, 13, 36, 37, 60, 61, 84, 85, 108, 109, 132, 133, 156, 157, 180, 181, 204,
                205, 228, 229, 252, 253, 276, 277, 300, 301, 324, 325, 348, 349, 372, 373 },
            new []{ 14, 15, 38, 39, 62, 63, 86, 87, 110, 111, 134, 135, 158, 159, 182, 183, 206,
                207, 230, 231, 254, 255, 278, 279, 302, 303, 326, 327, 350, 351, 374, 375 },
            new []{ 16, 17, 40, 41, 64, 65, 88, 89, 112, 113, 136, 137, 160, 161, 184, 185, 208,
                209, 232, 233, 256, 257, 280, 281, 304, 305, 328, 329, 352, 353, 376, 377 },
            new []{ 18, 19, 42, 43, 66, 67, 90, 91, 114, 115, 138, 139, 162, 163, 186, 187, 210,
                211, 234, 235, 258, 259, 282, 283, 306, 307, 330, 331, 354, 355, 378, 379 },
            new []{ 20, 21, 44, 45, 68, 69, 92, 93, 116, 117, 140, 141, 164, 165, 188, 189, 212,
                213, 236, 237, 260, 261, 284, 285, 308, 309, 332, 333, 356, 357, 380, 381 },
            new []{ 22, 23, 46, 47, 70, 71, 94, 95, 118, 119, 142, 143, 166, 167, 190, 191, 214,
                215, 238, 239, 262, 263, 286, 287, 310, 311, 334, 335, 358, 359, 382, 383 }
        };
        private static readonly int[][] NP2MuxTable = new int[][]
        {
            new []{ 0, 1, 32, 33, 64, 65, 96, 97, 128, 129, 160, 161, 192,
                193, 224, 225, 256, 257, 288, 289, 320, 321, 352, 353 },
            new []{ 2, 3, 34, 35, 66, 67, 98, 99, 130, 131, 162, 163, 194,
                195, 226, 227, 258, 259, 290, 291, 322, 323, 354, 355 },
            new []{ 4, 5, 36, 37, 68, 69, 100, 101, 132, 133, 164, 165, 196,
                197, 228, 229, 260, 261, 292, 293, 324, 325, 356, 357 },
            new []{ 6, 7, 38, 39, 70, 71, 102, 103, 134, 135, 166, 167, 198,
                199, 230, 231, 262, 263, 294, 295, 326, 327, 358, 359 },
            new []{ 8, 9, 40, 41, 72, 73, 104, 105, 136, 137, 168, 169, 200,
                201, 232, 233, 264, 265, 296, 297, 328, 329, 360, 361 },
            new []{ 10, 11, 42, 43, 74, 75, 106, 107, 138, 139, 170, 171, 202,
                203, 234, 235, 266, 267, 298, 299, 330, 331, 362, 363 },
            new []{ 12, 13, 44, 45, 76, 77, 108, 109, 140, 141, 172, 173, 204,
                205, 236, 237, 268, 269, 300, 301, 332, 333, 364, 365 },
            new []{ 14, 15, 46, 47, 78, 79, 110, 111, 142, 143, 174, 175, 206,
                207, 238, 239, 270, 271, 302, 303, 334, 335, 366, 367 },
            new []{ 16, 17, 48, 49, 80, 81, 112, 113, 144, 145, 176, 177, 208,
                209, 240, 241, 272, 273, 304, 305, 336, 337, 368, 369 },
            new []{ 18, 19, 50, 51, 82, 83, 114, 115, 146, 147, 178, 179, 210,
                211, 242, 243, 274, 275, 306, 307, 338, 339, 370, 371 },
            new []{ 20, 21, 52, 53, 84, 85, 116, 117, 148, 149, 180, 181, 212,
                213, 244, 245, 276, 277, 308, 309, 340, 341, 372, 373 },
            new []{ 22, 23, 54, 55, 86, 87, 118, 119, 150, 151, 182, 183, 214,
                215, 246, 247, 278, 279, 310, 311, 342, 343, 374, 375 },
            new []{ 24, 25, 56, 57, 88, 89, 120, 121, 152, 153, 184, 185, 216,
                217, 248, 249, 280, 281, 312, 313, 344, 345, 376, 377 },
            new []{ 26, 27, 58, 59, 90, 91, 122, 123, 154, 155, 186, 187, 218,
                219, 250, 251, 282, 283, 314, 315, 346, 347, 378, 379 },
            new []{ 28, 29, 60, 61, 92, 93, 124, 125, 156, 157, 188, 189, 220,
                221, 252, 253, 284, 285, 316, 317, 348, 349, 380, 381 },
            new []{ 30, 31, 62, 63, 94, 95, 126, 127, 158, 159, 190, 191, 222,
                223, 254, 255, 286, 287, 318, 319, 350, 351, 382, 383 }
        };

        // Dictionary containing the mux table for each part number
        public static readonly Dictionary<string, int[][]> MuxTables = new()
        {
            { NP1PartNumber1, NP1MuxTable },
            { NP1PartNumber2, NP1MuxTable },
            { NP2PartNumber, NP2MuxTable }
        };

    }
}
