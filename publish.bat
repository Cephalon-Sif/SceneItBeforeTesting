pushd "C:\Users\elsie\source\repos\SceneItBeforeTesting\SIB Design"
dotnet publish -c release -r linux-arm64 --self-contained "SIB Design.csproj"
cd bin\release\net8.0\linux-arm64\publish
CHOICE /C YN /M "Did you stop the server???"
scp -i \Users\elsie\.ssl\ssh-key-2025-03-15.key * opc@170.9.230.81:/opt/SIB-server/
scp -i \Users\elsie\.ssl\ssh-key-2025-03-15.key wwwroot\* opc@170.9.230.81:/opt/SIB-server/wwwroot/
scp -i \Users\elsie\.ssl\ssh-key-2025-03-15.key wwwroot\WebGLBuild\* opc@170.9.230.81:/opt/SIB-server/wwwroot/WebGLBuild/
scp -i \Users\elsie\.ssl\ssh-key-2025-03-15.key wwwroot\WebGLBuild\Build\* opc@170.9.230.81:/opt/SIB-server/wwwroot/WebGLBuild/Build
scp -i \Users\elsie\.ssl\ssh-key-2025-03-15.key wwwroot\WebGLBuild\TemplateData\* opc@170.9.230.81:/opt/SIB-server/wwwroot/WebGLBuild/TemplateData
echo "You can restart the server now"
popd