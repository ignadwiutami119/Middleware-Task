using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Task {

    class Program {
        public static int Main (string[] args) {
            string readToken = System.IO.File.ReadLines("token.txt").Last();
            var token = readToken;
            Token Token = new Token();
            var todoApp = new CommandLineApplication()
            {
                Name = "todo list",
                Description = "write and read a todo list",
                ShortVersionGetter = () => "1.0.0",
            };

        todoApp.Command("todo", app =>
             {
                 app.Description = "List Kegiatan";
                 var list = app.Option("--list", "show list", CommandOptionType.NoValue);
                 var clear = app.Option("--clear", "clear list", CommandOptionType.NoValue);
                 var add = app.Option("--add", "add list", CommandOptionType.SingleOrNoValue);
                 var update = app.Option("--update", "update list", CommandOptionType.MultipleValue);
                 var delete = app.Option("--delete", "delete list", CommandOptionType.SingleOrNoValue);
                 var done = app.Option("--done", "done list", CommandOptionType.SingleOrNoValue);

                 app.OnExecuteAsync(async cancellationToken =>
                 {
                     HttpClientHandler clientHandler = new HttpClientHandler();
                     clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                     {
                         return true;
                     };
                     HttpClient client = new HttpClient(clientHandler);
                     HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5001/todo");


                     if (list.HasValue())
                     {
                         if (token != "")
                         {
                             client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                             HttpResponseMessage response = await client.SendAsync(request);
                             var json = await response.Content.ReadAsStringAsync();

                             var obj = JsonConvert.DeserializeObject<List<Posts>>(json);

                             Console.WriteLine("Todo List : ");
                             foreach (var list in obj)
                             {
                                 if (list.status == true)
                                 {
                                     Console.WriteLine(list.id + ". " + list.list + " = Done");
                                 }
                                 else
                                 {
                                     Console.WriteLine(list.id + ". " + list.list + " = Undone");
                                 }
                             }
                         }
                     }

                     if (clear.HasValue())
                     {
                         client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                         HttpResponseMessage response = await client.SendAsync(request);
                         var json = await response.Content.ReadAsStringAsync();

                         var sure = Prompt.GetYesNo("Sure ?", false);
                         if (sure)
                         {
                                 await client.DeleteAsync("https://localhost:5001/todo/clear");
                         }
                     }

                     if (add.HasValue())
                     {
                         client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                         var todo = new Posts()
                         {
                             list = add.Value()
                         };
                         var json = JsonConvert.SerializeObject(todo);
                         var content = new StringContent(json, Encoding.UTF8, "application/json");
                         await client.PostAsync("https://localhost:5001/todo", content);

                     }

                     if (update.HasValue())
                     {
                         client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                         var todo = "{"
                                     + $"\"list\":\"{update.Values[1]}\""
                                     + "}";
                         var id = Convert.ToInt32(update.Values[0]);
                         var content = new StringContent(todo, Encoding.UTF8, "application/json");
                         await client.PatchAsync("https://localhost:5001/todo/" + id, content);
                     }

                     if (delete.HasValue())
                     {
                         client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                         var id = delete.Value();
                         await client.DeleteAsync("https://localhost:5001/todo/delete/" + Convert.ToInt32(id));
                     }

                     if (done.HasValue())
                     {
                         client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                         var todo = "{"
                                  + "\"status\":"
                                  + "true"
                                  + "}";
                         var content = new StringContent(todo, Encoding.UTF8, "application/json");
                         var id = Convert.ToInt32(done.Value());
                         await client.PatchAsync("https://localhost:5001/todo/done/" + id, content);
                     }
                 });
             });

              todoApp.Command("user", app =>
             {
                 app.Description = "Login or Register User";
                 var login = app.Option("--login", "Login User", CommandOptionType.MultipleValue);
                 var register = app.Option("--register", "Register User", CommandOptionType.MultipleValue);

                 app.OnExecuteAsync(async cancellationToken =>
                 {
                     HttpClientHandler clientHandler = new HttpClientHandler();
                     clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                     {
                         return true;
                     };
                     HttpClient client = new HttpClient(clientHandler);
                     HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5001/user");

                     if (register.HasValue ()) {
                        Console.WriteLine ("Enter username : ");
                        var uname = Console.ReadLine ();
                        Console.WriteLine ("Enter password : ");
                        var pass = Console.ReadLine ();
                        User user = new User () {
                            Username = uname,
                            Password = pass
                        };
                        var toJson = JsonConvert.SerializeObject (user);
                        var cnt = new StringContent (toJson, Encoding.UTF8, "application/json");
                        var posts = await client.PostAsync ("https://localhost:5001/user/register", cnt);
                    }

                    if (login.HasValue ()) {
                        Console.WriteLine ("Enter username : ");
                        var usr = Console.ReadLine ();
                        Console.WriteLine ("Enter password : ");
                        var pss = Console.ReadLine ();
                        User cek = new User () {
                            Username = usr,
                            Password = pss
                        };
                        var dt = JsonConvert.SerializeObject (cek);
                        var content = new StringContent (dt, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync ("https://localhost:5001/user/login", content);
                        var get = await response.Content.ReadAsStringAsync ();
                        var token = JsonConvert.DeserializeObject<Token> (get);
                        Console.WriteLine (token.token);
                        token.SaveToken ();
                    }
                 });
             });

            todoApp.OnExecute(() =>
            {
                todoApp.ShowHelp();
            });
            return todoApp.Execute(args);
        }
    }
                    
    class User {
        public int id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public ICollection<Posts> Post { get; set; }
    }

    public class Posts {
        public int id { get; set; }
        public string list { get; set; }
        public bool status { get; set; }
        public int UserId { get; set; }
    }

    public class Token {
        public string token { get; set; }

        public string GetSavedToken () {
            return System.IO.File.ReadAllText ("token.txt");
        }

        public bool SaveToken () {
            if (token != null) {
                var fToken = new StreamWriter ("token.txt");
                fToken.WriteLine (token);
                fToken.Close ();
                return true;
            } else
                return false;
        }
    }
}