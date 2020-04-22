#region samples_get_actors
// Sample for http://fastvideocataloger.com  
//css_ref WindowsBase;
//css_ref PresentationCore;
//css_ref PresentationFramework;
//css_ref System.Runtime;
//css_ref System.ObjectModel;

using System;
using System.Xaml;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Web;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VideoCataloger
{

    public partial class GetActors : Window
    {
        // api documented here: https://developers.themoviedb.org/
        static string api_key = ""; // replace with your api key from https://www.themoviedb.org/
        static string api_base_url = "https://api.themoviedb.org/3/";
        static IScripting m_scripting = null;

        static public async Task Run(IScripting scripting, string argument)
        {
            m_scripting = scripting;

            if (api_key=="")
            {
                m_scripting.GetConsole().WriteLine("Replace api_key with your personal api key from https://www.themoviedb.org/");
                return;
            }

            GetActors wnd = new GetActors();
            wnd.Show();
        }


        DependencyObject m_RootElement;

        RemoteCatalogService.Actor m_CurrentActor = null;

        public GetActors()
        {
            m_scripting.GetConsole().Clear();

            Width = 640;
            Height = 480;
            Title = "Search actors at themoviedb.org";

            // load the xaml code to be used as content for this window
            string current_folder = Directory.GetCurrentDirectory();
            string path = current_folder + "\\scripts\\samples\\get_actors.xaml";
            using (FileStream fs = new FileStream( path, FileMode.Open))
            {
                m_RootElement = (DependencyObject) System.Windows.Markup.XamlReader.Load(fs);
            }

            // setup events for elements in xaml
            Button btn = (Button)LogicalTreeHelper.FindLogicalNode(m_RootElement, "search_btn");
            btn.Click += Search_Click;

            Button add_btn = (Button)LogicalTreeHelper.FindLogicalNode(m_RootElement, "add_btn");
            add_btn.Click += AddToCatalog_Click;
            
            // use the loaded scene
            this.Content = m_RootElement;
        }

        void SetStatus( string new_status )
        {
            Label ui_name = (Label)LogicalTreeHelper.FindLogicalNode(m_RootElement, "status");
            ui_name.Content = new_status;
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            HideResults();

            TextBox textbox = (TextBox) LogicalTreeHelper.FindLogicalNode(m_RootElement, "search_box");
            SetStatus("searching...");
            CPersonSearchResult search_results = await SearchActors( textbox.Text );

            if (search_results != null)
            {
                if (search_results.total_pages > 0)
                {
                    SetStatus("processing results...");

                    string name = search_results.results[0].name;

                    string url = await MakeProfileImageUrl(search_results.results[0].profile_path);
                    byte[] image_data = await GetImageData(url);

                    m_CurrentActor = new RemoteCatalogService.Actor();
                    string first_name = name;
                    string last_name = "";

                    int space = name.IndexOf(" ");
                    if (space != -1)
                    {
                        first_name = name.Substring(0, space).Trim();
                        last_name = name.Substring(space + 1).Trim();
                    }
                    m_CurrentActor.FirstName = first_name;
                    m_CurrentActor.LastName = last_name;
                    m_CurrentActor.Image = image_data;

                    // we now have portrait, name and an id... lets get more info on this person.
                    CPersonResult person = await GetPerson(search_results.results[0].id);
                    m_CurrentActor.Bio = person.biography;
                    m_CurrentActor.Link = person.homepage;

                    BitmapImage image = null;
                    MemoryStream image_stream = null;
                    if (image_data != null)
                    {
                        image_stream = new MemoryStream(image_data);
                        image = LoadImageFromStream(image_stream);
                        SetActorToUI(name, m_CurrentActor.Bio, image);
                        image_stream.Close();
                    }
                }
            }

            SetStatus("");
        }

        private void AddToCatalog_Click(object sender, RoutedEventArgs e)
        {
            if (m_CurrentActor == null)
                return;

            var catalog = m_scripting.GetVideoCatalogService();
            int actor_id = catalog.AddActorToDB(m_CurrentActor);
            if (actor_id != -1)
            {
                Button add_btn = (Button)LogicalTreeHelper.FindLogicalNode(m_RootElement, "add_btn");
                add_btn.Visibility = Visibility.Collapsed;
            }
            m_scripting.GetGUI().Refresh("");
        }

        private void SetActorToUI(string name, string desc, BitmapImage portrait_image )
        {
            Image ui_portrait = (Image)LogicalTreeHelper.FindLogicalNode(m_RootElement, "actor_portrait");
            ui_portrait.Source = portrait_image;
            Label ui_name = (Label)LogicalTreeHelper.FindLogicalNode(m_RootElement, "actor_name");
            ui_name.Content = name;
            TextBlock ui_desc = (TextBlock)LogicalTreeHelper.FindLogicalNode(m_RootElement, "actor_desc");
            ui_desc.Text = desc;

            ShowResults();
        }

        void HideResults()
        {
            DockPanel ui_results = (DockPanel)LogicalTreeHelper.FindLogicalNode(m_RootElement, "ui_results");
            ui_results.Visibility = Visibility.Collapsed;
            Button add_btn = (Button)LogicalTreeHelper.FindLogicalNode(m_RootElement, "add_btn");
            add_btn.Visibility = Visibility.Collapsed;
        }

        void ShowResults()
        {
            DockPanel ui_results = (DockPanel)LogicalTreeHelper.FindLogicalNode(m_RootElement, "ui_results");
            ui_results.Visibility = Visibility.Visible;
            Button add_btn = (Button)LogicalTreeHelper.FindLogicalNode(m_RootElement, "add_btn");
            add_btn.Visibility = Visibility.Visible;
        }




        [DataContract]
        public class CConfiguration
        {
            [DataMember]
            public CImages images{ get; set; }
            [DataMember]
            public string[] change_keys { get; set; }
        };

        [DataContract]
        public class CImages
        {
            [DataMember]
            public string base_url { get; set; }
            [DataMember]
            public string secure_base_url { get; set; }
            [DataMember]
            public string[] backdrop_sizes { get; set; }
            [DataMember]
            public string[] logo_sizes { get; set; }
            [DataMember]
            public string[] poster_sizes { get; set; }
            [DataMember]
            public string[] profile_sizes { get; set; }
            [DataMember]
            public string[] still_sizes { get; set; }
        };

        CConfiguration m_Configuration = null;


        private async Task<CConfiguration> GetConfiguration()
        {
            if (m_Configuration != null)
                return m_Configuration;

            string config_search_line = api_base_url + "configuration?api_key=" + api_key;
            m_Configuration = await GetFromJSON<CConfiguration>(config_search_line);
            return m_Configuration;
        }

        private async Task<string> MakeProfileImageUrl( string profile_path )
        {
            CConfiguration config = await GetConfiguration();

            string url = config.images.base_url;

            url += config.images.profile_sizes[config.images.profile_sizes.Length-1];   // last one is largest and original size

            url += profile_path;

            return url;
        }

        static private BitmapImage LoadImageFromStream(Stream stream)
        {
            if (stream == null)
                return null;

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }

        private async Task<byte[]> GetImageData(string url)
        {
            byte[] image_data = null;
            try
            {
                Uri image_url = new Uri(url);
                var webClient = new System.Net.WebClient();
                image_data = await webClient.DownloadDataTaskAsync(image_url);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return image_data;
        }

        public async Task<T> GetFromJSON<T>(string query)
        {
            T parsed_stuct = default(T);
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            var client = new HttpClient(handler);
            try
            {
                var videoGetIndexRequestResult = await client.GetAsync(query);

                var result = videoGetIndexRequestResult.Content.ReadAsStringAsync().Result;

                var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(result));

                var serializer = new DataContractJsonSerializer(typeof(T));
                parsed_stuct = (T)serializer.ReadObject(ms);
                ms.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return parsed_stuct;
        }

        public class CPersonSearchResult
        {
            [DataMember]
            public int page { get; set; }

            [DataMember]
            public int total_results { get; set; }

            [DataMember]
            public int total_pages { get; set; }

            [DataMember]
            public CActorResult[] results { get; set; }

        };

        [DataContract]
        public class CActorResult
        {
            [DataMember]
            public string name { get; set; }

            [DataMember]
            public int id { get; set; }

            [DataMember]
            public string profile_path { get; set; }
        };

        private async Task<CPersonSearchResult> SearchActors( string query )
        {
            string encoded_query = System.Net.WebUtility.UrlEncode( query );

            string search_line = api_base_url + "search/person?api_key=" + api_key + "&language=en-US&query=" + encoded_query + "&include_adult=true&page=1";

            CPersonSearchResult header_result = await GetFromJSON<CPersonSearchResult>(search_line);
            return header_result;
        }

        [DataContract]
        public class CPersonResult
        {
            [DataMember]
            public string biography { get; set; }

            [DataMember]
            public string imdb_id { get; set; }

            [DataMember]
            public string homepage { get; set; }
        };

        async Task<CPersonResult> GetPerson( int id )
        {
            string query_line = api_base_url + "person/" + id + "?api_key=" + api_key + "&language=en-US";

            CPersonResult person = await GetFromJSON<CPersonResult>(query_line);

            return person;
        }
    }
}
#endregion
