# NuGet Package References

This directory contains a dummy project that references all the NuGet packages used by the Unity package.
On publish, it copies the transitive closure of all referenced packages in to '../Assets/Plugins'.
This should be done every time packages are updated, by running `script/update-nuget-packages`.