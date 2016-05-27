using System.Collections.Generic;
using System.Windows;
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

                if(o.translation != null)
                {
                    var translation = string.Join(", ", o.translation.ToArray());
                    var title = translation;
                    if (o.basic?.phonetic != null)
                    {
                        title += " [" + o.basic.phonetic + "]";
                    }
                    results.Add(new Result
                    {
                        Title = title,
                        SubTitle = "翻译结果",
                        IcoPath = ico,
                        Action = this.copyToClipboardFunc(translation)
                    });
                }

                if(o.basic?.explains != null)
                {
                    var explantion = string.Join(",", o.basic.explains.ToArray());
                    results.Add(new Result
                    {
                        Title = explantion,
                        SubTitle = "简明释义",
                        IcoPath = ico,
                        Action = this.copyToClipboardFunc(explantion)
                    });
                }

                if (o.web != null)
                {
                    foreach (WebTranslation t in o.web)
                    {
                        var translation = string.Join(",", t.value.ToArray());
                        results.Add(new Result
                        {
                            Title = translation,
                            SubTitle = "网络释义："+ t.key,
                            IcoPath = ico,
                            Action = this.copyToClipboardFunc(translation)
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

        private System.Func<ActionContext, bool> copyToClipboardFunc(string text)
        {
            return c =>
            {
                if (this.copyToClipboard(text))
                {
                    _context.API.ShowMsg("翻译已被存入剪贴板");
                }
                else
                {
                    _context.API.ShowMsg("剪贴板打开失败，请稍后再试");
                }
                return false;
            };
        }

        private bool copyToClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }catch(System.Exception e)
            {
                return false;
            }
            return true;
        }
    }
}
