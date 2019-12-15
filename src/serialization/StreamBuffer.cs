using System;

namespace sne
{

    public class StreamBuffer
    {
        private byte[] _data;
        private int _capacity;
        private int _readIndex = 0;
        private int _writeIndex = 0;

        public byte[] data { get { return _data; } }
        public int readIndex {
            get { return _readIndex; }
            set { _readIndex = value; }
        }
        public int writeIndex {
            get { return _writeIndex; }
            set { _writeIndex = value; }
        }

        public StreamBuffer(int capacity) {
            _capacity = capacity;
            _data = new byte[_capacity];
        }

        // 아이템을 버퍼 끝에 추가한다
        public void write(byte item) {
            reserve(sizeof(byte));
            _data[_writeIndex] = item;
            ++_writeIndex;
        }

        // 버퍼 앞에서 아이템 하나를 읽는다
        public byte read() {
            if (empty()) {
                throw new StreamException();
            }

            byte value = _data[_readIndex];
            ++_readIndex;
            if (empty()) {
                reset();
            }
            return value;
        }

        // 버퍼를 초기화한다
        public void reset() {
            _readIndex = _writeIndex = 0;
        }

        public bool crunch() {
            if (_readIndex != 0) {
                if (_readIndex > _writeIndex) {
                    throw new StreamException(); // 로직 오류
                }

                int length = size();
                if (length > 0) {
                    Buffer.BlockCopy(_data, _readIndex, _data, 0, length);
                }
                _readIndex = 0;
                _writeIndex = length;
            }
            return true;
        }


        // 외부 버퍼로 부터 메모리를 복사한다
        public void copyFrom(byte[] bytes, int bytesOffset, int count) {
            if (count <= 0) {
                throw new StreamException("Invalid count: " + count.ToString());
            }
            reserve(count);
            Buffer.BlockCopy(bytes, bytesOffset, _data, _writeIndex, count);
            _writeIndex += count;
        }

        // 외부 버퍼로 바이트 스트림을 복사한다
        public void copyTo(byte[] bytes, int count) {
            if (size() < count) {
                throw new StreamException("Not enough space: " + count.ToString());
            }

            Buffer.BlockCopy(_data, _readIndex, bytes, 0, count);
            _readIndex += count;

            if (empty()) {
                reset();
            }
        }

        // 여유 공간 크기를 얻는다
        public int space() {
            return _capacity - _writeIndex;
        }

        // 버퍼가 비어 있는가?
        public bool empty() {
            return _readIndex == _writeIndex;
        }

        // 버퍼의 크기를 얻는다
        public int size() {
            return _writeIndex - _readIndex;
        }

        private void reserve(int neededSize) {
            if (space() < neededSize) {
                _capacity *= 2;
                Array.Resize(ref _data, _capacity);
                if (space() < neededSize) {
                    throw new StreamException("out of memory");
                }
            }
        }
    }

} // namespace sne
