
namespace Com.Bcom.Solar.Gprc
{
    public class ResultStatus
    {
        public bool Success { get; } = true;
        public string ErrMessage { get; } = "";

        public ResultStatus(bool success, string errMessage)
        {
            this.Success = success;
            this.ErrMessage = errMessage;
        }
    }
}

