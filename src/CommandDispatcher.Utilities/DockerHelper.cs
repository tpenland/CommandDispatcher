using Docker.DotNet;
using Docker.DotNet.Models;
using System.Runtime.InteropServices;

namespace CommandDispatcher.Utilities;

public class DockerHelper : IDisposable
{
    private readonly DockerClient _client;
    private readonly List<string> _startedContainerIds = new();
    public static DockerHelper Instance { get; } = new();

    public const string LinuxDockerProcessName = "dockerd";
    public const string WindowsDockerProcessName = "docker";

    private readonly object _lockObject = new();

    private DockerHelper()
    {
        // In linux if a container is running its process shows up a process named 'docker' but the actual process is 'dockerd'
        string dockerProcessName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? WindowsDockerProcessName : LinuxDockerProcessName;

        var dockerProcess = System.Diagnostics.Process.GetProcessesByName(dockerProcessName);
        if (dockerProcess is null || dockerProcess.Length == 0)
        {
            throw new DockerPluginNotFoundException(System.Net.HttpStatusCode.NotFound, "Docker process is not running");
        }
        _client = new DockerClientConfiguration().CreateClient();
    }

    public async Task<ImagesListResponse?> FindImage(string imageName)
    {
        IList<ImagesListResponse> images = await _client.Images.ListImagesAsync(new ImagesListParameters());
        ImagesListResponse? foundImage = images.FirstOrDefault(i => i.RepoTags?.Contains(imageName) ?? false);
        return foundImage;
    }

    public async Task<ContainerListResponse?> FindContainer(string containerName)
    {
        IList<ContainerListResponse> containers = await _client.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
        ContainerListResponse? foundContainer = containers.FirstOrDefault(c => c.Names.Contains(containerName));
        return foundContainer;
    }

    public async Task<ContainerListResponse?> FindContainerByImageName(string imageName)
    {
        IList<ContainerListResponse> containers = await _client.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
        ContainerListResponse? foundContainer = containers.FirstOrDefault(c => c.Image.Contains(imageName));
        return foundContainer;
    }

    public async Task RemoveContainer(string imageName)
    {
        ContainerListResponse? foundContainer = await FindContainerByImageName(imageName);
        if (foundContainer is not null)
        {
            await _client.Containers.StopContainerAsync(foundContainer.ID, new ContainerStopParameters());
            await _client.Containers.RemoveContainerAsync(foundContainer.ID, new ContainerRemoveParameters());
        }
    }

    public async Task RemoveImage(string imageId)
    {
        await _client.Images.DeleteImageAsync(imageId, new ImageDeleteParameters());
    }

    public void StartContainer(CreateContainerParameters containerParameters)
    {
        lock (_lockObject)
        {
            DockerHelper.Instance.RunContainer(containerParameters).Wait();
        }
    }

    private async Task RunContainer(CreateContainerParameters containerParameters)
    {
        // - If container exists, but is not running, start it
        // - else, if image exists, create container and start it
        // - else, pull image, create container and start it
        ContainerListResponse? foundContainer = await FindContainer(containerParameters.Name);
        foundContainer ??= await FindContainerByImageName(containerParameters.Image);

        if (foundContainer is null)
        {
            ImagesListResponse? foundImage = await FindImage(containerParameters.Image);
            if (foundImage is null)
            {
                await _client.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = containerParameters.Image }, null, new Progress<JSONMessage>());
            }
            CreateContainerResponse response = await _client.Containers.CreateContainerAsync(containerParameters);
            await _client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());
            _startedContainerIds.Add(response.ID);
        }
        else
        {
            if (foundContainer.State != "running")
            {
                await _client.Containers.StartContainerAsync(foundContainer.ID, new ContainerStartParameters());
                _startedContainerIds.Add(foundContainer.ID);
            }
        }
    }

    public void StartMosquitto(string configLocation)
    {
        var createContainerParams = new CreateContainerParameters
        {
            Image = "eclipse-mosquitto:latest",
            Name = "/mosquitto",
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            "1883/tcp",
                            new List<PortBinding>
                            {
                                new PortBinding
                                {
                                    HostPort = "1883"
                                }
                            }
                        }
                    },
                Binds = new List<string>
                    {
                        $"{configLocation}:/mosquitto/config/mosquitto.conf"
                    },
                //AutoRemove = true,
            }
        };

        StartContainer(createContainerParams);
    }

    public async Task Cleanup()
    {
        foreach (string id in _startedContainerIds)
        {
            await _client.Containers.StopContainerAsync(id, new ContainerStopParameters());
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
        }
    }
}