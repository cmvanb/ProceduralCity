# ProceduralCity
A procedural city generator.

## Submodules
In order to preserve a nice file structure for Unity, the git submodules are not cloned in to the `src/Assets/` directory directly. Instead they are placed at `src/Submodules/` and a symbolic link is made from there to the appropriate Unity directory. This decreases the number of irrelevant files (such as the submodule's .gitignore) and reduces the depth of the file structure.

### Example
The UnityCommon submodule is symlinked from `src/Submodules/UnityCommon/src/Assets/UnityCommon/` to `src/Assets/Submodules/UnityCommon/`.

### Windows Setup
It is necessary to setup the submodules in order to run this Unity project.

1. Initialize the git submodules and update them recursively:  
`git submodule update --init --recursive`

2. Make a directory junction (type of symlink) on Windows:  
`mklink /j src\Assets\Submodules\UnityCommon src\Submodules\UnityCommon\src\Assets\UnityCommon`

Don't forget to update the submodules if there are upstream changes:

1. INSERT GIT COMMAND HERE
