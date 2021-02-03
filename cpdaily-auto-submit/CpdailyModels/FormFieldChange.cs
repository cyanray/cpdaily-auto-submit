using Newtonsoft.Json;

namespace cpdaily_auto_submit.CpdailyModels
{
    /// <summary>
    /// 描述 FormField 修改值
    /// </summary>
    public class FormFieldChange
    {
        /// <summary>
        /// 1:文本，2:单选，3:多选，4:图片(暂不支持)
        /// </summary>
        [JsonProperty("fieldType")]
        public int FieldType { get; set; }

        /// <summary>
        /// 用于合并时精确匹配，不匹配则无法合并
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 文本值或者选项的文本
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
