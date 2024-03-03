using Docker.DotNet.Models;
using CommandDispatcher.Utilities;

namespace CommandSubscriber.Utilities.Tests;

public class DockerHelperTests : IDisposable
{
    private bool disposedValue;

    [Fact]
    public async Task CreateContainer_RemoveImageTest()
    {
        string containerName = "busybox";
        string imageName = "busybox:latest";

        await DockerHelper.Instance.RemoveContainer(imageName);
        await DockerHelper.Instance.RemoveImage(imageName);

        var containerParams = new CreateContainerParameters { Image = imageName, Name = containerName, Cmd = new[] { "echo", "test test test" } };
        DockerHelper.Instance.StartContainer(containerParams);
        await Task.Delay(1000);
        ContainerListResponse? container = await DockerHelper.Instance.FindContainerByImageName(imageName);
        Assert.NotNull(container);
        Assert.Equal("exited", container.State);

        await DockerHelper.Instance.RemoveContainer(imageName);
        container = await DockerHelper.Instance.FindContainerByImageName(imageName);
        Assert.Null(container);
        await DockerHelper.Instance.RemoveImage(imageName);
        ImagesListResponse? image = await DockerHelper.Instance.FindImage(imageName);
        Assert.Null(image);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                DockerHelper.Instance.Cleanup().GetAwaiter().GetResult();
            }
            DockerHelper.Instance.Dispose();
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}