namespace Streamfox.Server
{
    using Microsoft.Extensions.DependencyInjection;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public static class BootstrappingExtensions
    {
        public static IServiceCollection AddVideoHosting(this IServiceCollection services)
        {
            var intermediateHandler = new DirectoryHandler("Intermediate");
            var thumbnailHandler = new DirectoryHandler("Thumbnails");
            var videoHandler = new DirectoryHandler("Videos");

            services.AddTransient(factory => new ThumbnailFileHandler(thumbnailHandler));
            services.AddTransient(factory => new VideoFileHandler(videoHandler));
            services.AddTransient<IThumbnailFileReader>(factory => factory.GetService<ThumbnailFileHandler>());
            services.AddTransient<IThumbnailFileWriter>(factory => factory.GetService<ThumbnailFileHandler>());
            services.AddTransient<IVideoFileContainer>(factory => factory.GetService<VideoFileHandler>());
            services.AddTransient<IVideoFileReader>(factory => factory.GetService<VideoFileHandler>());
            services.AddTransient<IVideoLoader, VideoLoaderFromDisk>();
            services.AddTransient<VideoRetrievalClerk>();
            services.AddTransient<IExistenceChecker>(factory => new ExistenceChecker(thumbnailHandler, videoHandler));
            services.AddTransient<IIntermediateVideoWriter>(factory => new IntermediateVideoWriter(intermediateHandler));
            services.AddTransient<IMultimediaFramework, MultimediaFramework>();
            services.AddTransient<IPathResolver>(
                    factory => new PathResolver(
                            intermediateHandler,
                            thumbnailHandler,
                            videoHandler));
            services.AddTransient<IProcessRunner, ProcessRunner>();
            services.AddTransient<IFfmpeg, Ffmpeg>();
            services.AddTransient<IVideoProcessor, VideoProcessor>();
            services.AddTransient<IVideoIdGenerator, SnowflakeVideoIdGenerator>();
            services.AddTransient<VideoStorageClerk>();
            services.AddTransient<IVideoClerk, VideoClerkFacade>();

            return services;
        }
    }
}