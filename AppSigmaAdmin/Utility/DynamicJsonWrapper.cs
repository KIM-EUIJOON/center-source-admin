using System.Runtime.Serialization;

namespace AppSigmaAdmin.Utility
{

    /// <summary>
    /// DynamicJsonによって返されるdynamic型を格納しておくためだけのWrapperクラス。
    /// 本当に素のdynamic型を使うと後々意味不明になる可能性が高いため。
    /// 
    /// </summary>
    public class DynamicJsonWrapper
    {
        /// <summary>
        /// dynamicJsonで変換されたdynamic型の値を取得する。
        /// </summary>
        public dynamic Content { get; set; }

        /// <summary>
        /// 空のdynamicJsonWrapperインスタンスを返す。
        /// </summary>
        public static DynamicJsonWrapper Empty()
        {
            return new DynamicJsonWrapper(DynamicJson.Parse("{}"));
        }

        /// <summary>
        /// json文字列からDynamicJsonでdynamic型のインスタンスを作成し、
        /// さらにそれをDynamicJsonWrapper型のインスタンスでくるんで返す。
        /// 出来る限りこちらでインスタンス化したい。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static DynamicJsonWrapper CreateByString(string s)
        {
            var dj = DynamicJson.Parse(s);
            return DynamicJsonWrapper.Create(dj);
        }

        /// <summary>
        /// DynamicJsonを使ってdynamic型にしたインスタンスを受け取る。
        /// dynamicJson型で変換したもの以外のdynamicなオブジェクトも受け取れてしまうため、出来るだけ使わない。
        /// </summary>
        /// <param name="jsonDynamic"></param>
        /// <returns></returns>
        public static DynamicJsonWrapper Create(dynamic jsonDynamic)
        {
            return new DynamicJsonWrapper(jsonDynamic);
        }

        public DynamicJsonWrapper(dynamic _DynamicJsonObject)
        {
            Content = _DynamicJsonObject;
        }

        /// <summary>
        /// DynamicJsonのシリアライズ機能を直接呼ぶ。
        /// シリアライズされたjson文字列が返る。
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return DynamicJson.Serialize(Content);
        }


        /// <summary>
        /// オブジェクトの配列に変換して返す。
        /// _property = new DynamicJsonWrapper(_crawlerJson.ToObjectArray().Select(x => DynamicJson.Parse(x.ToString())).Where(x => x.set_name == set_name).First<dynamic>());
        /// ↑これを使うときに便利。
        /// </summary>
        /// <returns></returns>
        public object[] ToObjectArray()
        {

            return ((object[])Content);

        }


    }
}