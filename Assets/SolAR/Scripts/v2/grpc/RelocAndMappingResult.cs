namespace Com.Bcom.Solar.Gprc
{
    public class RelocAndMappingResult
    {
        public ResultStatus Status;
        public RelocalizationResult Result;

        public RelocAndMappingResult(ResultStatus status, RelocalizationResult result)
        {
            this.Status = status;
            this.Result = result;
        }
    }
}