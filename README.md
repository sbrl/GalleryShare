# :framed_picture: :wave: GalleryShare
A simple program to share a directory of pictures and / or recursively. Inspired by [netham91/Quick-Share](https://github.com/netham91/Quick-Share).

## :package: Getting Started
To get started, head over to the [releases](https://github.com/sbrl/GalleryShare/releases/) page and download the latest release.

If you're on Linux or Mac, you'll need to install the [mono](http://www.mono-project.com/) runtime. Debian users can simply execute `sudo apt install mono-runtime` - users of other distributions can just substitute `apt` for their own package manager :smiley_cat:

If you're on Windows, you'e already got everything installed that you need, though it should be noted that due to a ridiculous quirk in the way `System.Net.HttpListener` works, you'll need to run GalleryShare as an administrator! If you use the mono runtime (Yes, it is available for Windows too!), you shouldn't experience this problem though.

Once you've got yourself setup, simply execute `GalleryShare.exe`, and it'll start the GalleryShare server. Type `./GalleryShare.exe --help` to learn more about the different options provided, such as a way to change the port GalleryShare listens on, or the diredctyory GalleryShare serves from.

## :hourglass_flowing_sand: Stay up to date!
If you like GalleryShare and want to keep it, add the folder GalleryShare resides in to your system's PATH, or create a batch / bash file that calls it for you. TO kepe up-to-date on GalleryShare releases, I find [Sibbell](https://about.sibbell.com/) useful for sending me notifications about new releases from my GitHub stars.
