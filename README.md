# Mandelbrot Explorer
![alt text](/Img/Program.jpg)
This program is written using WPF C# to explore the Mandelbrot Set. 

## What is Mandelbrot Set?

If you are here, then most likely you know what it is. If you do not know, then I strongly recommend that you read Wikipedia and a couple more sites:
- https://en.wikipedia.org/wiki/Mandelbrot_set
- https://en.wikipedia.org/wiki/Julia_set
- https://mathworld.wolfram.com/MandelbrotSet.html
- https://www.youtube.com/watch?v=FFftmWSzgmk (I love this guys)
- https://www.youtube.com/watch?v=FFftmWSzgmk

## How to install the program?

You can download the WPF project for VS or download the compiled program and immediately start exploring the world of fractals. To do this, download the `Installer` folder. In it you will find the ordinary program installer.

## How to use the program?

#### Position parametrs
I hope you read something about how the calculations go. First you need to choose the coordinates that can be found on the Internet. By the way, there are a lot of beautiful coordinates here http://www.cuug.ab.ca/dewara/mandelbrot/images.html. You can write like that `0,429` , or like that `2.87378E-5`, it doesn't matter
#### Render parametrs
Then choose the number of iterations, after which the cycle is interrupted. The main thing to know is that the closer you approach the set, the more iterations you need to set, otherwise you will get a **black picture**. For example, for a 2 x 2 image 100 iterations are enough, but for a image ![alt text](https://bit.ly/2JKKtkY) x ![alt text](https://bit.ly/2JKKtkY) **you need 2000 iterations**.
Select the resolution of your image. While looking for a beautiful frame, I advise you to set **no more than 500** (of course, if you do not have a supercomputer). And after that you can render in Full HD or 4K (it will take **several minutes**). Now you can press the start button. The program will freeze for a few seconds.
#### Coloring parametrs
This is the most difficult part of the setup, which requires imagination (and complex algorithms over which I racked my brains for you). When you change one of these parameters you need to click `Recoloring` button to apply the changes.
When you clicked on the start button, the program compiled a matrix, each cell of which shows on which iteration we realized that the point does not belong to the Mandelbrot set. Depending on this number, we will paint over each pixel. The program uses a method of control points. Let's consider his work on examples and you will understand how it works.
#### Example 1
![Example1](/Img/Exe1.png)
We work with three variables: 
- Iterations in cycle `cycleIter=1000`
- Shift `shift=0`
- And for each pixel we know how many iterations `i` we needed to understand that it does not belong to the Mandelbrot set (or belongs to)

In this example we have two control points {0;white} and {1;black}.
For each pixel we are calculating a number using the simple formula `num=[(i+shift) mod cycleIter]/cycleIter` (In fact, I use a more complex formula, but they are very close in value).
The `num` takes values from 0 to 1. **So don't create control point with another range!!!** We look at which control point it is closer and select the appropriate color. If our `num` 0.5, then the pixel will be gray, and if 0 - then white.
#### Example 2
![Example2](/Img/Exe2.png)
>But what does `shift` mean?
This example is almost no different. I'd changed only the `shift`.
After analyzing the formula and looking at an example, you can understand that the `shift` simply shifts the color of each pixel: white becomes black and black becomes white
#### Example 3
![Example3](/Img/Exe4.png)
Finally, we got rid of depressive shades of gray.
I hope you understand how coloring works. Good Luck!

