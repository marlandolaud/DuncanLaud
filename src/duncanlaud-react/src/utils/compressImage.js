/**
 * Compress and resize an image file on the client side before uploading.
 *
 * Uses an off-screen canvas to downscale the image to fit within
 * maxDimension (default 800px) and re-encodes it as JPEG at the
 * specified quality (default 0.80 → ~80 %).
 *
 * @param {File} file        – the original File from an <input type="file">
 * @param {object} [opts]
 * @param {number} [opts.maxDimension=800]  – longest edge in pixels
 * @param {number} [opts.quality=0.80]      – JPEG quality 0-1
 * @returns {Promise<File>}  – a compressed JPEG File ready for FormData
 */
export default function compressImage(file, { maxDimension = 200, quality = 0.50 } = {}) {
  return new Promise((resolve, reject) => {
    const img = new Image();
    const url = URL.createObjectURL(file);

    img.onload = () => {
      URL.revokeObjectURL(url);

      let { width, height } = img;

      // Only downscale, never upscale
      if (width > maxDimension || height > maxDimension) {
        if (width >= height) {
          height = Math.round(height * (maxDimension / width));
          width = maxDimension;
        } else {
          width = Math.round(width * (maxDimension / height));
          height = maxDimension;
        }
      }

      const canvas = document.createElement('canvas');
      canvas.width = width;
      canvas.height = height;

      const ctx = canvas.getContext('2d');
      ctx.drawImage(img, 0, 0, width, height);

      canvas.toBlob(
        (blob) => {
          if (!blob) {
            reject(new Error('Image compression failed.'));
            return;
          }
          // Preserve a meaningful filename but switch extension to .jpg
          const name = file.name.replace(/\.[^.]+$/, '') + '.jpg';
          resolve(new File([blob], name, { type: 'image/jpeg' }));
        },
        'image/jpeg',
        quality,
      );
    };

    img.onerror = () => {
      URL.revokeObjectURL(url);
      reject(new Error('Could not read image file.'));
    };

    img.src = url;
  });
}
