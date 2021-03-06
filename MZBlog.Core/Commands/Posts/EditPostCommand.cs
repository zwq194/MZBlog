﻿using MongoDB.Driver.Linq;
using MZBlog.Core.Documents;
using MZBlog.Core.Extensions;
using System;
using System.Linq;

namespace MZBlog.Core.Commands.Posts
{
    public class EditPostCommand
    {
        public string PostId { get; set; }

        public string AuthorId { get; set; }

        public string Title { get; set; }

        public string TitleSlug { get; set; }

        public string MarkDown { get; set; }

        public string Tags { get; set; }

        public DateTime PubDate { get; set; }

        public bool Published { get; set; }
    }

    public class EditPostCommandInvoker : ICommandInvoker<EditPostCommand, CommandResult>
    {
        private readonly MongoCollections _collections;

        public EditPostCommandInvoker(MongoCollections collections)
        {
            _collections = collections;
        }

        public CommandResult Execute(EditPostCommand command)
        {
            var blogPostCol = _collections.BlogPostCollection;

            var post = blogPostCol.AsQueryable().FirstOrDefault(p => p.Id == command.PostId);

            if (post == null)
                throw new ApplicationException("Post with id: {0} was not found".FormatWith(command.PostId));
            var markdown = new MarkdownSharp.Markdown();
            //TODO:应该验证TitleSlug是否是除了本文外唯一的

            post.MarkDown = command.MarkDown;
            post.Content = markdown.Transform(command.MarkDown);
            post.PubDate = command.PubDate.CloneToUtc();
            post.Status = command.Published ? PublishStatus.Published : PublishStatus.Draft;
            post.Title = command.Title;
            post.TitleSlug = command.TitleSlug.Trim().ToSlug();
            if (!command.Tags.IsNullOrWhitespace())
            {
                post.Tags = command.Tags.Trim().Split(',')
                .Select(t => new Tag { Name = t.Trim(), Slug = t.Trim().ToSlug() })
                .ToArray();
            }
            else
                post.Tags = new Tag[] { };

            blogPostCol.Save(post);

            return CommandResult.SuccessResult;
        }
    }
}