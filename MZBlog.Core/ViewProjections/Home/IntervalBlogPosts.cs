﻿using MongoDB.Driver.Linq;
using MZBlog.Core.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MZBlog.Core.ViewProjections.Home
{
    public class IntervalBlogPostsViewModel
    {
        public IEnumerable<BlogPost> Posts { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }

    public class IntervalBlogPostsBindingModel
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }

    public class IntervalBlogPostsViewProjection : IViewProjection<IntervalBlogPostsBindingModel, IntervalBlogPostsViewModel>
    {
        private readonly MongoCollections _collections;

        public IntervalBlogPostsViewProjection(MongoCollections collections)
        {
            _collections = collections;
        }

        public IntervalBlogPostsViewModel Project(IntervalBlogPostsBindingModel input)
        {
            var posts = _collections.BlogPostCollection.AsQueryable()
                     .Where(BlogPost.IsPublished)
                     .Where(b => b.PubDate < input.ToDate && b.PubDate > input.FromDate)
                     .OrderByDescending(b => b.PubDate)
                     .ToList();

            return new IntervalBlogPostsViewModel
            {
                Posts = posts,
                FromDate = input.FromDate,
                ToDate = input.ToDate
            };
        }
    }
}