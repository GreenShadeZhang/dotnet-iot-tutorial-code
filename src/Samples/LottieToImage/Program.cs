using Verdure.LottieToImage;

await LottieToImage.SaveLottieFramesAsync(
           "printer.json",  // Lottie动画文件路径
           "output",         // 输出目录
           240,             // 宽度
           240             // 高度
       );
