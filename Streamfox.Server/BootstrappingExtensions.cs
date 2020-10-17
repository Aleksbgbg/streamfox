namespace Streamfox.Server
{
    using Microsoft.Extensions.DependencyInjection;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.Processing;
    using Streamfox.Server.Processing.Ffmpeg;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public static class BootstrappingExtensions
    {
        public static IServiceCollection AddVideoHosting(this IServiceCollection services)
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

            services.AddTransient<IMetadataRetriever>(
                    factory => new DiskMetadataStore(
                            fileWriter: metadataFileStore,
                            fileReader: metadataFileStore));
            services.AddTransient<IMetadataSaver>(
                    factory => new DiskMetadataStore(
                            fileWriter: metadataFileStore,
                            fileReader: metadataFileStore));
            services.AddTransient<IVideoLoader>(
                    factory => new DiskVideoLoader(
                            fileLister: metadataFileStore,
                            fileExistenceChecker: metadataFileStore,
                            videoFileReadOpener: videoFileStore,
                            thumbnailFileReadOpener: thumbnailFileStore));
            services.AddTransient<IIntermediateVideoWriter>(
                    factory => new IntermediateVideoWriter(
                            fileStreamWriter: intermediateFileStore,
                            fileDeleter: intermediateFileStore));
            services.AddTransient<IVideoComponentExistenceChecker>(
                    factory => new VideoComponentExistenceCheckerFacade(
                            videoFileExistenceChecker: videoFileStore,
                            thumbnailFileExistenceChecker: thumbnailFileStore));
            services.AddTransient<IVideoComponentPathResolver>(
                    factory => new VideoComponentPathResolverFacade(
                            intermediateVideoFilePathResolver: intermediateFileStore,
                            thumbnailFilePathResolver: thumbnailFileStore,
                            videoFilePathResolver: videoFileStore));
            services.AddTransient<IVideoIdGenerator, SnowflakeVideoIdGenerator>();

            services.AddSingleton<VideoConversionQueue>();
            services.AddHostedService<BackgroundVideoConverter>();
            services.AddSingleton<IVideoConverter>(factory => factory.GetService<VideoConversionQueue>());

            services.AddTransient<IFfmpegProcessRunner, FfmpegProcessRunner>();
            services.AddTransient<IVideoProcessor, VideoProcessor>();
            services.AddTransient<VideoRetrievalClerk>();
            services.AddTransient<VideoStorageClerk>();
            services.AddTransient<IVideoClerk, VideoClerkFacade>();

            return services;
        }
    }
}