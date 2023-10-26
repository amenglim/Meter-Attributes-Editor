namespace Meter_Attributes_Editor.Resources.SVN
{
    /// <summary>
    /// SVN revision info
    /// </summary>
    public class SvnInfo
    {
        /// <summary>
        /// SVN repo revision
        /// </summary>
        public const string REVISION = "$WCREV$";

        /// <summary>
        /// If the code was modified
        /// </summary>
        public const string MODIFIED = "$WCMODS?1:0$";

        /// <summary>
        /// SVN repo this project is in
        /// </summary>
        public const string REPOSITORY_URL = @"$WCURL$";

        /// <summary>
        /// SVN repo date
        /// </summary>
        public const string REPOSITORY_DATE = "$WCDATE$";

        /// <summary>
        /// Date and time program was built
        /// </summary>
        public const string BUILD_DATE = "$WCNOW$";
    }
}