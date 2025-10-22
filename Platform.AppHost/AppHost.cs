using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq", 5341)
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");

var redis = builder.AddRedis("storage")
    .WithImagePullPolicy(ImagePullPolicy.Missing)
    .WithRedisInsight();

var orleans = builder.AddOrleans("orleans")
    .WithClustering(redis)
    .WithGrainStorage("Default", redis);

var silo = builder.AddProject<Platform_Silo>("silo")
    .WithReference(orleans)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(seq)
    .WaitFor(seq)
    .WithReplicas(3);

var apiService = builder.AddProject<Platform_ApiService>("apiservice")
    .WithReference(orleans.AsClient())
    .WithReference(redis)
    .WaitFor(silo)
    .WithReference(seq)
    .WaitFor(seq)
    .WithHttpHealthCheck("/health");

builder.AddProject<Platform_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
