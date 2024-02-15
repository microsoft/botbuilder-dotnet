// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v$templateversion$

using $ext_safeprojectname$;

namespace $ext_safeprojectname$.Tests.Dialogs.TestData
{
    /// <summary>
    /// A class to store test case data for <see cref="BookingDialogTests"/>.
    /// </summary>
    public class BookingDialogTestCase
    {
        public string Name { get; set; }

        public BookingDetails InitialBookingDetails { get; set; }

        public string[,] UtterancesAndReplies { get; set; }

        public BookingDetails ExpectedBookingDetails { get; set; }
    }
}
