using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NettruyenRemake.Helpers
{
    public class ImageHelper
    {
        // Tải ảnh từ URL và trả về byte[]
        public static async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                    return imageBytes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image: {ex.Message}");
                using (var httpClient = new HttpClient())
                {
                    var errorBytes = await httpClient.GetByteArrayAsync("https://scontent.fhan18-1.fna.fbcdn.net/v/t1.6435-9/128851635_5427040923980029_649285277980215436_n.png?_nc_cat=102&ccb=1-7&_nc_sid=6ee11a&_nc_ohc=AiCn1rWRq3YQ7kNvgGcix4D&_nc_oc=AdmR24MN1D0M6d4j5H8i-FkHVLqSGFblHUsgdOPH0f8qE3O4bjR2Mq2VAtr2K3f2jVA&_nc_zt=23&_nc_ht=scontent.fhan18-1.fna&_nc_gid=IbSFYAQbuuXiS1-44qdIAA&oh=00_AYEV2z_SIdAlEJgKBpQpWVs7te_27CQ6s8aZnQUhQna4Yw&oe=680BF505");
                    return errorBytes;
                }
            }
        }
    }
}
