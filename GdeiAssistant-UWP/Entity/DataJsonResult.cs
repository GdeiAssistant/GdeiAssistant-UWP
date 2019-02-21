namespace GdeiAssistant.Entity
{
    public class DataJsonResult<T> : JsonResult
    {
        public T data { set; get; }
    }
}
