using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZWave4Net.Channel.Protocol;

namespace ZWave4Net.Channel
{
    public class ZWaveChannel : IObserver<Frame>
    {
        private readonly FrameReceiver _receiver;
        private readonly FrameTransmitter _transmitter;

        private readonly CancellationTokenSource _cancellationSource;
        public readonly IByteStream Stream;
        
        public ZWaveChannel(IByteStream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));

            _cancellationSource = new CancellationTokenSource();

            var reader = new FrameReader(Stream);
            var writer = new FrameWriter(stream);

            _receiver = new FrameReceiver(reader, _cancellationSource.Token);
            _transmitter = new FrameTransmitter(writer, _cancellationSource.Token);
        }

        public void Open()
        {
            _receiver.Start();
        }

        public void Close()
        {
            _cancellationSource.Cancel();

            _receiver.Wait(TimeSpan.FromSeconds(1));
            _transmitter.Wait(TimeSpan.FromSeconds(1));
        }

        void IObserver<Frame>.OnCompleted()
        {
        }

        void IObserver<Frame>.OnError(Exception error)
        {
        }

        void IObserver<Frame>.OnNext(Frame value)
        {
            if (value is EventDataFrame eventData)
            {
                _transmitter.Transmit(Frame.ACK);
            }
        }
    }
}
