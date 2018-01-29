# ProceduralCity
A procedural city generator.

# Submodules
In order to preserve a nice file structure for Unity, the git submodules are not cloned in to the `src/Assets/` directory directly. Instead they are placed at `src/Submodules/` and a symbolic link is made from there to the appropriate Unity directory. This also avoids having irrelevant files (such as the submodule's .gitignore) present in the Unity file structure.

## Example
The UnityCommon submodule is symlinked from `src/Submodules/UnityCommon/src/Assets/UnityCommon/` to `src/Assets/Submodules/UnityCommon/`.

## Windows Setup
Because the symlinked files are included under version control, it should not be necessary to set up the submodules in order to run this program. However if you intend to develop, or you expect that the submodule will receive upstream changes, you should set it up.

1. Remove the existing submodule files:
`rd /s /q src\Assets\Submodules\UnityCommon\`

2. Initialize the git submodules and update them recursively:
`git submodule update --init --recursive`

3. Make a directory junction (type of symlink) on Windows:
`mklink /j src\Assets\Submodules\UnityCommon src\Submodules\UnityCommon\src\Assets\UnityCommon`
