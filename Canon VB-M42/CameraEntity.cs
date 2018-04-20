using Stationary.Fixing.Grab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Canon_VB_M42
{
    public class CameraEntity
    {
        public string RtspStream => "rtsp://192.168.100.100:554/stream/profile0";
        /// <summary>
        /// Rtsp порт камеры
        /// </summary>
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Модель устройства
        /// </summary>
        public CameraModel Model { get; set; }

        /// <summary>
        /// Учетная запись камеры
        /// </summary>
        public string Login { get; set; }
        /// <summary>
        /// Пароль учетной записи камеры
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Зашифрованный пароль для хранения
        /// </summary>
        /// <summary>
        /// Адрес камеры
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// Http порт камеры
        /// </summary>
        public int HttpPort { get; set; } = 80;
        /// <summary>
        /// Rtsp порт камеры
        /// </summary>
        public int RtspPort { get; set; } = 554;

        /// <summary>
        /// Тайм-аут
        /// </summary>
        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 0, 10);

        /// <summary>
        /// Тип захвата изображения с камеры
        /// </summary>
        public string GrabberType { get; set; }
        /// <summary>
        /// Тип управления камерой
        /// </summary>
        public string PtzType { get; set; }


        /// <summary>
        /// Базовый http адрес
        /// </summary>
        public string BaseHttp => $"http://{IpAddress}:{HttpPort}";

        /// <summary>
        /// Rtsp поток
        /// </summary>

        /// <summary>
        /// Учетные данные
        /// </summary>
        public NetworkCredential Credential =>
            (string.IsNullOrWhiteSpace(Login) && string.IsNullOrWhiteSpace(Password)) ?
            null : new NetworkCredential(Login, Password);

    }
}
