# Ocean Globe

I was curious how [Polka Dot Pirate](https://managore.itch.io/polka-dot-pirate) implemented its "ocean globe" effect, so I did some research and spent a couple hours building out this similar effect in Unity.

![Screenshot](http://i.imgur.com/jJJVJOq.gif)

The Gerstner Wave code was adapted from [this GPU Gems chapter](http://http.developer.nvidia.com/GPUGems/gpugems_ch01.html), and the terrain is generated from a simple Perlin octave function I've used in other games. The flat plane is wrapped around a semisphere using a buch of Trigonometry functions that I'm glad I worked through, because my Trig is rusty after 15 years of non-use.

I don't intend to do anything further with this, feel free to use any of the code under MIT license.