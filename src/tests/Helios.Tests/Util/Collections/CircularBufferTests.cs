﻿using System;
using System.Linq;
using System.Text;
using Helios.Util.Collections;
using Xunit;

namespace Helios.Tests.Util.Collections
{
    public class CircularBufferTests
    {
        protected virtual ICircularBuffer<T> GetBuffer<T>(int capacity, int maxCapacity)
        {
            return new CircularBuffer<T>(capacity, maxCapacity);
        }

        protected virtual ICircularBuffer<T> GetBuffer<T>(int capacity)
        {
            return new CircularBuffer<T>(capacity);
        }
            
        [Fact]
        public void CircularBuffer_should_expand()
        {
            var initialBuffer = GetBuffer<int>(10,30);
            for(var i = 0; i < 20; i++)
                initialBuffer.Enqueue(i);
            Assert.Equal(30, initialBuffer.Capacity);
            Assert.Equal(20, initialBuffer.Size);
        }

        [Fact]
        public void CircularBuffer_should_shrink()
        {
            var initialBuffer = GetBuffer<int>(10, 30);
            for (var i = 0; i < 20; i++)
                initialBuffer.Enqueue(i);
            initialBuffer.Capacity = 5;
            Assert.Equal(5, initialBuffer.Capacity);
            Assert.Equal(5, initialBuffer.Size);
        }

        /// <summary>
        /// If a circular buffer is defined with a fixed maximum capacity, it should
        /// simply overwrite the old elements even if they haven't been dequed
        /// </summary>
        [Fact]
        public void CircularBuffer_should_not_expand()
        {
            var byteArrayOne = Encoding.Unicode.GetBytes("ONE STRING");
            var byteArrayTwo = Encoding.Unicode.GetBytes("TWO STRING");
            var buffer = GetBuffer<byte>(byteArrayOne.Length);
            buffer.Enqueue(byteArrayOne);
            Assert.Equal(buffer.Size, buffer.Capacity);
            buffer.Enqueue(byteArrayTwo);
            Assert.Equal(buffer.Size, buffer.Capacity);
            var availableBytes = buffer.DequeueAll().ToArray();
            Assert.False(byteArrayOne.SequenceEqual(availableBytes));
            Assert.True(byteArrayTwo.SequenceEqual(availableBytes));
        }

        [Fact]
        public void CircularBuffer_should_keep_adding_elements_after_Max_Capacity()
        {
            var numbers = Enumerable.Range(1, 10000).ToList();
            var finalNumbers = Enumerable.Range(9801, 200).ToList();
            var buffer = GetBuffer<int>(5, 200);
            foreach (var number in numbers)
            {
                buffer.Enqueue(number);
            }
            var resultNumbers = buffer.DequeueAll();
            Assert.True(finalNumbers.SequenceEqual(resultNumbers));
        }

        [Fact]
        public void CircularBuffer_should_access_correct_element_at_specified_index()
        {
            var numbers = Enumerable.Range(1, 10000).ToList();
            var buffer = GetBuffer<int>(5, 200);
            foreach (var number in numbers.Take(10))
            {
                buffer.Enqueue(number);
            }
            Assert.True(buffer.IsElementAt(2));
            Assert.Equal(3, buffer.ElementAt(2));
            buffer.Dequeue();
            Assert.Equal(4, buffer.ElementAt(2));
            Assert.False(buffer.IsElementAt(11));
            foreach (var number in numbers.Skip(10).Take(100))
            {
                buffer.Enqueue(number);
            }
            Assert.True(buffer.IsElementAt(11));
            Assert.Equal(13, buffer[11]);
        }

        /// <summary>
        /// Used in high-performance scenarios - under the hood it uses Array.Copy to page objects
        /// in memory from source buffer into the CircularBuffers' internal buffer.
        /// </summary>
        [Fact]
        public void CircularBuffer_should_accept_write_directly_into_buffer()
        {
            var initialContents = Encoding.Unicode.GetBytes("This is a relatively short unicode string");
            var nextContents = Encoding.Unicode.GetBytes("12");
            var buffer = GetBuffer<byte>(initialContents.Length);
            buffer.DirectBufferWrite(initialContents);
            Assert.Equal(initialContents.Length, buffer.Size);
            
            //Try to add new bytes - these should go to the front of the buffer
            buffer.DirectBufferWrite(nextContents);
            Assert.Equal(initialContents.Length, buffer.Size); //should should still not have changed
            var bytes = buffer.Dequeue(4); //grab the new bytes we wrote from the front of the buffer
            Assert.True(nextContents.SequenceEqual(bytes));
        }

        /// <summary>
        /// In situations where we don't have any contiguous room to write into a buffer, we should be able to wrap around the front of it
        /// </summary>
        [Fact]
        public void CircularBuffer_should_accept_write_overflow_directly_into_buffer()
        {
            var initialContents = Encoding.Unicode.GetBytes("This is a relatively short unicode string");
            var nextContents = Encoding.Unicode.GetBytes("12");
            var buffer = GetBuffer<byte>(initialContents.Length + 1); //add some extra padding, so we're one byte short of capacity after write #1
            buffer.DirectBufferWrite(initialContents);
            Assert.Equal(initialContents.Length, buffer.Size);

            //Try to add new bytes - these should go to the front of the buffer
            buffer.DirectBufferWrite(nextContents);
            Assert.Equal(initialContents.Length + 1, buffer.Size); //should should still not have changed
            var bytes = new[]{ buffer[buffer.Size-1]}.Concat(buffer.Dequeue(3)); //grab the new bytes we wrote from the back and front of the buffer
            Assert.True(nextContents.SequenceEqual(bytes));
        }

        [Fact]
        public void CircularBuffer_should_read_directly_from_buffer()
        {
            var initialContents = Encoding.Unicode.GetBytes("This is a relatively short unicode string");
            var buffer = GetBuffer<byte>(initialContents.Length);
            buffer.DirectBufferWrite(initialContents);
            Assert.Equal(initialContents.Length, buffer.Size);

            //read directly from the buffer
            var destBuffer = new Byte[initialContents.Length];
            buffer.DirectBufferRead(destBuffer);
            Assert.True(initialContents.SequenceEqual(destBuffer));
            Assert.Equal(0, buffer.Size);
        }

        /// <summary>
        /// Should be able to perform a wrap-around read and still grab a contiguous chunk of the buffer
        /// </summary>
        [Fact]
        public void CircularBuffer_should_read_directly_from_NonContiguous_buffer()
        {
            var initialContents = Encoding.Unicode.GetBytes("This is a relatively short unicode string");
            var nextContents = Encoding.Unicode.GetBytes("12");
            var buffer = GetBuffer<byte>(initialContents.Length + 1); //add some extra padding, so we're one byte short of capacity after write #1
            buffer.DirectBufferWrite(initialContents);
            Assert.Equal(initialContents.Length, buffer.Size);

            //Read the old content from the buffer... (move read-head forward to Capacity - 1)
            var destBuffer = new Byte[initialContents.Length];
            buffer.DirectBufferRead(destBuffer);
            Assert.True(initialContents.SequenceEqual(destBuffer));
            Assert.Equal(0, buffer.Size);

            //Try to add new bytes - these should go to the front of the buffer
            buffer.DirectBufferWrite(nextContents);
            var bytes = new Byte[4];

            //Read directly from the buffer
            buffer.DirectBufferRead(bytes);
            Assert.True(nextContents.SequenceEqual(bytes));
            Assert.Equal(0, buffer.Size);
        }

        [Fact]
        public void CircularBuffer_should_Read_and_Write_Directly_from_Expanding_Buffer()
        {
            var initialContents = Encoding.Unicode.GetBytes("This is a relatively short unicode string");
            var nextContents = Encoding.Unicode.GetBytes("12");
            var buffer = GetBuffer<byte>(4, initialContents.Length*2); //add some extra padding, so we're one byte short of capacity after write #1
            buffer.DirectBufferWrite(initialContents);

            //Read the old content from the buffer... (move read-head forward to Capacity - 1)
            var destBuffer = new Byte[initialContents.Length];
            buffer.DirectBufferRead(destBuffer);
            Assert.True(initialContents.SequenceEqual(destBuffer));
            Assert.Equal(0, buffer.Size);
        }
    }
}
