namespace Streamfox.Server
{
    using Microsoft.Extensions.DependencyInjection;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.Persistence.Database;
    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.Processing;
    using Streamfox.Server.Processing.Ffmpeg;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public static class BootstrappingExtensions
    {
        public static IServiceCollection AddVideoHosting(this IServiceCollection services)
        {
            AddVideoClerks(services);
            AddHighLevelVideoProcessors(services);
            AddLowLevelVideoProcessors(services);
            AddFfmpeg(services);
            AddPersistence(services);

            return services;
        }

        private static void AddVideoClerks(IServiceCollection services)
        {
            services.AddTransient<VideoStorageClerk>();
            services.AddTransient<VideoRetrievalClerk>();
            services.AddTransient<VideoProgressClerk>();
            services.AddTransient<IVideoClerk, VideoClerkFacade>();
        }

        private static void AddHighLevelVideoProcessors(IServiceCollection services)
        {
            services.AddTransient<IVideoIdGenerator, SnowflakeVideoIdGenerator>();
            services.AddTransient<IVideoProcessor, VideoProcessor>();
            services.AddTransient<IVideoVerifier, VideoVerifier>();
            services.AddTransient<IBackgroundVideoProcessor, BackgroundVideoProcessor>();
            services.AddTransient<IFormatConverter, ProgressLoggingFormatConverter>();
            services.AddTransient<IVideoFinalizer, VideoFinalizer>();
            services.AddTransient<IClockFactory, StopwatchClockFactory>();
            services.AddSingleton<VideoProgressStore>();
            services.AddTransient<IVideoProgressStore>(
                    factory => factory.GetService<VideoProgressStore>());
            services.AddTransient<IProgressSink>(
                    factory => factory.GetService<VideoProgressStore>());
            services.AddTransient<IProgressRetriever>(
                    factory => factory.GetService<VideoProgressStore>());
            services.AddTransient<IVideoPathResolver>(
                    factory => factory.GetService<VideoComponentPathResolverFacade>());
            services.AddTransient<IIntermediateVideoPathResolver>(
                    factory => factory.GetService<VideoComponentPathResolverFacade>());
            services.AddTransient<IThumbnailPathResolver>(
                    factory => factory.GetService<VideoComponentPathResolverFacade>());
        }

        private static void AddLowLevelVideoProcessors(IServiceCollection services)
        {
            services.AddTransient<ITaskRunner, TaskRunner>();
            services.AddTransient<IThumbnailExtractor, ThumbnailExtractor>();
            services.AddTransient<IMetadataExtractor, MetadataExtractor>();
            services.AddTransient<IFormatConverterWithLogging, FormatConverter>();
            services.AddTransient<IFramesFetcher, FramesFetcher>();
        }

        private static void AddFfmpeg(IServiceCollection services)
        {
            services.AddTransient<IVideoFramesFetcher, FfmpegProcessVideoOperationRunner>();
            services
                    .AddTransient<IFileSystemThumbnailExtractor, FfmpegProcessVideoOperationRunner
                    >();
            services.AddTransient<IVideoMetadataGrabber, FfmpegProcessVideoOperationRunner>();
            services.AddTransient<IVideoCoercer, FfmpegProcessVideoOperationRunner>();
            services.AddTransient<IFfmpegProcessRunner, FfmpegProcessRunner>();
        }

        private static void AddPersistence(IServiceCollection services)
        {
            AddFileStores(services);
            services.AddTransient<IMetadataRetriever, DatabaseMetadataStore>();
            services.AddTransient<IMetadataSaver, DatabaseMetadataStore>();
            services.AddTransient<IThumbnailExistenceChecker>(
                    factory => factory.GetService<DiskVideoLoader>());
            services.AddTransient<IVideoExistenceChecker>(
                    factory => factory.GetService<VideoDatabaseContext>());
            services.AddTransient<IVideoLister>(
                    factory => factory.GetService<VideoDatabaseContext>());
        }

        private static void AddFileStores(IServiceCollection services)
        {
            FileStore intermediateFileStore = new FileStore("Intermediate");
            FileStore metadataFileStore = new FileStore("Metadata");
            FileStore thumbnailFileStore = new FileStore("Thumbnails");
            FileStore videoFileStore = new FileStore("Videos");

            foreach (FileStore fileStore in new[]
            {
                intermediateFileStore, metadataFileStore, thumbnailFileStore, videoFileStore
            })
            {
                fileStore.EnsureFileStorePresent();
            }

            services.AddTransient<DiskVideoLoader>(
                    factory => new DiskVideoLoader(
                            fileLister: metadataFileStore,
                            videoFileReadOpener: videoFileStore,
                            thumbnailFileReadOpener: thumbnailFileStore,
                            thumbnailExistenceChecker: thumbnailFileStore));
            services.AddTransient<IVideoLoader>(factory => factory.GetService<DiskVideoLoader>());
            services.AddTransient(
                    factory => new IntermediateVideoWriter(
                            fileStreamWriter: intermediateFileStore,
                            fileDeleter: intermediateFileStore));
            services.AddTransient<IIntermediateVideoWriter>(
                    factory => factory.GetService<IntermediateVideoWriter>());
            services.AddTransient<IIntermediateVideoDeleter>(
                    factory => factory.GetService<IntermediateVideoWriter>());
            services.AddTransient<IVideoComponentExistenceChecker>(
                    factory => new VideoComponentExistenceCheckerFacade(
                            videoFileExistenceChecker: videoFileStore,
                            thumbnailFileExistenceChecker: thumbnailFileStore));
            services.AddTransient(
                    factory => new VideoComponentPathResolverFacade(
                            intermediateVideoFilePathResolver: intermediateFileStore,
                            thumbnailFilePathResolver: thumbnailFileStore,
                            videoFilePathResolver: videoFileStore));
        }
    }
}