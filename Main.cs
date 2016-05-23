using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using Wox.Infrastructure.Http;

namespace Wox.Plugin.Youdao
{
    public class TranslateResult
    {
        public int errorCode { get; set; }
        public List<string> translation { get; set; }
        public BasicTranslation basic { get; set; }
        public List<WebTranslation> web { get; set; }
    }

    // 有道词典-基本词典
    public class BasicTranslation
    {
        public string phonetic { get; set; }
        public List<string> explains { get; set; }
    }

    public class WebTranslation
    {
        public string key { get; set; }
        public List<string> value { get; set; }
    }

    public class Main : IPlugin
    {
        private const string TranslateUrl = "http://fanyi.youdao.com/openapi.do?keyfrom=WoxLauncher&key=1247918016&type=data&doctype=json&version=1.1&q=";
        private PluginInitContext _context;

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            const string ico = "Images\\youdao.ico";
            if (query.Search.Length == 0)
            {
                results.Add(new Result
                {
                    Title = "开始有道中英互译",
                    SubTitle = "基于有道网页 API",
                    IcoPath = ico
                });
                return results;
            }
            var json = Http.Get(TranslateUrl + query.Search, _context.Proxy).Result;
            TranslateResult o = JsonConvert.DeserializeObject<TranslateResult>(json);
            if (o.errorCode == 0)
            {
                if (o.basic?.phonetic != null)
                {
                    var explantion = string.Join(",", o.basic.explains.ToArray());
                    results.Add(new Result
                    {
                        Title = o.basic.phonetic,
                        SubTitle = explantion,
                        IcoPath = ico,
                        Action = c =>
                        {
                            Clipboard.SetText(explantion);
                            _context.API.ShowMsg("解释已被存入剪贴板");
                            return false;
                        }
                    });
                }
                foreach (string t in o.translation)
                {
                    results.Add(new Result
                    {
                        Title = t,
                        IcoPath = ico,
                        Action = c =>
                        {
                            Clipboard.SetText(t);
                            _context.API.ShowMsg("翻译已被存入剪贴板");
                            return false;
                        }
                    });
                }
                if (o.web != null)
                {
                    foreach (WebTranslation t in o.web)
                    {
                        var translation = string.Join(",", t.value.ToArray());
                        results.Add(new Result
                        {
                            Title = t.key,
                            SubTitle = translation,
                            IcoPath = ico,
                            Action = c =>
                            {
                                Clipboard.SetText(t.key);
                                _context.API.ShowMsg("网络翻译已被存入剪贴板");
                                return false;
                            }
                        });
                    }
                }
            }
            else
            {
                string error = string.Empty;
                switch (o.errorCode)
                {
                    case 20:
                        error = "要翻译的文本过长";
                        break;

                    case 30:
                        error = "无法进行有效的翻译";
                        break;

                    case 40:
                        error = "不支持的语言类型";
                        break;

                    case 50:
                        error = "无效的key";
                        break;
                }

                results.Add(new Result
                {
                    Title = error,
                    IcoPath = ico
                });
            }

            return results;
        }

        public void Init(PluginInitContext context)
        {
            _context = context;
        }
    }
}
