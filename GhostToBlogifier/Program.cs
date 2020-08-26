using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blogifier.Core.Data;
using GhostToBlogifier.GhostExport;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GhostToBlogifier
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            Startup startup = new Startup();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var postRepository = serviceProvider.GetService<IPostRepository>();
            var authorRepository = serviceProvider.GetService<IAuthorRepository>();
            var ghostbackup = File.ReadAllText(@"dsr_backup.json");
            var export = JsonConvert.DeserializeObject<GhostExport.GhostExport>(ghostbackup);
            foreach (var db in export.db)
            {
                var postItem = new PostItem();
                var posts = db.data.posts.Where(p => p.page == 0).ToArray();
                foreach (var post in posts)
                {
                    string[] tags = GetTags(db, post);
                    postItem.Content = post.html;
                    postItem.Description = post.title;
                    postItem.Author = await authorRepository.GetItem(a => a.Id == Int32.Parse(post.author_id));
                    postItem.Categories = string.Join(", ", tags);
                    postItem.Slug = post.slug;
                    postItem.Title = post.title;
                    if (post.published_at != null) postItem.Published = DateTime.Parse(post.published_at);
                    if (post.feature_image != null) postItem.Cover = post.feature_image;
                    var save = await postRepository.SaveItem(postItem);
                    Console.WriteLine($"Adding post: {post.title}");
                }
            }
        }
        private static string[] GetTags(Db db, Post post)
        {
            var tagIds = db.data.posts_tags.Where(x => x.post_id == post.id)
                .Select(x => x.tag_id)
                .ToArray();

            var tags = db.data.tags.Where(t => tagIds.Contains(t.id))
                .Select(t => t.name)
                .ToArray();
            return tags;
        }
    }
}
