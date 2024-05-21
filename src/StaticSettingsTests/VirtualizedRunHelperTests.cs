public class VirtualizedRunHelperTests :
    IDisposable
{
    [Fact]
    public void Built_on_windows_run_on_linux_wsl()
    {
        VirtualizedRunHelper.Env = new TestEnv(
            _ => _ switch
            {
                "/mnt/c/build/project_dir" => true,
                "/mnt/c/build/project_dir/src/file.cs" => true,
                _ => false
            },
            "/mnt/c/build/project_dir/subdir");

        var helper = new VirtualizedRunHelper(@"C:\build\project_dir", string.Empty);

        Assert.True(helper.AppearsToBeLocalVirtualizedRun);
        Assert.True(helper.Initialized);

        Assert.Equal("/mnt/c/build/project_dir/src/file.cs", helper.GetMappedBuildPath(@"C:\build\project_dir\src\file.cs"));
    }

    [Fact]
    public void Built_on_windows_run_on_linux_ci()
    {
        VirtualizedRunHelper.Env = new TestEnv(
            _ => _ switch
            {
                "/ci/build/project_dir" => true,
                "/ci/build/project_dir/src/file.cs" => true,
                _ => false
            },
            "/ci/build/project_dir/subdir");

        var helper = new VirtualizedRunHelper(@"C:\build\project_dir", @"C:\build\project_dir");

        Assert.True(helper.AppearsToBeLocalVirtualizedRun);
        Assert.True(helper.Initialized);

        Assert.Equal(
            "/ci/build/project_dir/src/file.cs",
            helper.GetMappedBuildPath(@"C:\build\project_dir\src\file.cs"));
    }

    [Fact]
    public void Built_on_windows_run_on_linux_ci_fallback()
    {
        VirtualizedRunHelper.Env = new TestEnv(
            _ => _ switch
            {
                "/mnt/c/Users/jairb/Downloads/TestVerify" => true,
                "/mnt/c/Users/jairb/Downloads/TestVerify/src/file.cs" => true,
                _ => false
            },
            "/mnt/c/Users/jairb/Downloads/TestVerify");

        var helper = new VirtualizedRunHelper(@"C:\Users\jairb\Downloads\TestVerify", @"C:\Users\jairb\Downloads\TestVerify\TestVerify");

        Assert.True(helper.AppearsToBeLocalVirtualizedRun);
        Assert.True(helper.Initialized);

        Assert.Equal(
            "/mnt/c/Users/jairb/Downloads/TestVerify/src/file.cs",
            helper.GetMappedBuildPath(@"C:\Users\jairb\Downloads\TestVerify\src\file.cs"));
    }

    [Fact]
    public void Built_on_windows_run_on_linux_docker()
    {
        VirtualizedRunHelper.Env = new TestEnv(
            _ => _ switch
            {
                "/mnt/approot/build-outputs" => true,
                "/mnt/approot/src/file.cs" => true,
                _ => false
            },
            "/mnt/approot/build-outputs");

        var helper = new VirtualizedRunHelper(@"C:\my-src\contoso\proj-x", @"C:\my-src\contoso\proj-x");

        Assert.False(helper.AppearsToBeLocalVirtualizedRun);
        Assert.False(helper.Initialized);

        Assert.Equal("/mnt/approot/src/file.cs", helper.GetMappedBuildPath(@"C:\my-src\contoso\proj-x\src\file.cs"));

        Assert.True(helper.AppearsToBeLocalVirtualizedRun);
        Assert.True(helper.Initialized);
    }

    [Fact]
    public void Built_on_windows_run_on_windows()
    {
        VirtualizedRunHelper.Env = new TestEnv(
            _ => _ switch
            {
                @"C:\proj\file.cs" => true,
                @"C:\proj" => true,
                _ => false
            },
            @"C:\proj",
            '\\');

        var helper = new VirtualizedRunHelper(@"C:\proj", @"C:\proj");

        Assert.False(helper.AppearsToBeLocalVirtualizedRun);
        Assert.True(helper.Initialized);

        Assert.Equal("C:\\d\\e", helper.GetMappedBuildPath("C:\\d\\e"));
    }

    public void Dispose() =>
        VirtualizedRunHelper.Env = PhysicalEnvironment.Instance;

    class TestEnv(Func<string, bool> exists, string currentDirectory, char directorySeparatorChar = '/')
        : IEnvironment
    {
        public string CurrentDirectory { get; } = currentDirectory;
        public char DirectorySeparatorChar { get; } = directorySeparatorChar;

        public char AltDirectorySeparatorChar
        {
            get
            {
                if (DirectorySeparatorChar == '/')
                {
                    return '\\';
                }

                return '/';
            }
        }

        public bool PathExists(string path) =>
            exists(path);
        public string CombinePaths(string path1, string path2) =>
            Path.Combine(path1, path2);
    }
}