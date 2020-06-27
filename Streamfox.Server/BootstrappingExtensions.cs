namespace Streamfox.Server
{
    using Microsoft.Extensions.DependencyInjection;

    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoManagement.Persistence;
    using Streamfox.Server.VideoManagement.Processing;

    public static class BootstrappingExtensions
    {
        public static IServiceCollection AddVideoHosting(this IServiceCollection services)
        {
            services.AddTransient<IVideoSnapshotter, FfmpegProcessVideoSnapshotter>();
            services.AddTransient(factory => new ThumbnailFileHandler(new DirectoryHandler("Thumbnails")));
            services.AddTransient(factory => new VideoFileHandler(new DirectoryHandler("Videos")));
            services.AddTransient<IThumbnailFileReader>(factory => factory.GetService<ThumbnailFileHandler>());
            services.AddTransient<IThumbnailFileWriter>(factory => factory.GetService<ThumbnailFileHandler>());
            services.AddTransient<IVideoFileContainer>(factory => factory.GetService<VideoFileHandler>());
            services.AddTransient<IVideoFileReader>(factory => factory.GetService<VideoFileHandler>());
            services.AddTransient<IVideoFileWriter>(factory => factory.GetService<VideoFileHandler>());
            services.AddTransient<IVideoLoader, VideoLoaderFromDisk>();
            services.AddTransient<VideoRetrievalClerk>();
            services.AddTransient<IVideoSaver, VideoSaverToDisk>();
            services.AddTransient<IVideoIdGenerator, SnowflakeVideoIdGenerator>();
            services.AddTransient<VideoStorageClerk>();
            services.AddTransient<IVideoClerk, VideoClerkFacade>();

            return services;
        }
    }
}