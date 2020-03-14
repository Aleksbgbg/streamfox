namespace Streamfox.Server
{
    using Microsoft.Extensions.DependencyInjection;

    public static class BootstrappingExtensions
    {
        public static IServiceCollection AddVideoHosting(this IServiceCollection services)
        {
            services.AddTransient<VideoDirectoryHandler>();
            services.AddTransient<IFileSystemChecker>(factory => factory.GetService<VideoDirectoryHandler>());
            services.AddTransient<IFileSystemManipulator>(factory => factory.GetService<VideoDirectoryHandler>());
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