using System;
using UnityEngine;
namespace GP
{
    [Serializable]
    /**
     * @brief Range of frames.
     * @note Start is _not_ guaranteed to be smaller or equal to End, it is up to the user to make sure.
     */
    public struct FrameRange
    {
        /// @brief Returns the start frame.
        public int Start;

        /// @brief Returns the end frame.
        public int End;

        /// @brief Sets / Gets the length.
        /// @note It doesn't cache the value.
        public int Length { set { End = Start + value; } get { return End - Start; } }

        /**
         * @brief Create a frame range
         * @param start Start frame
         * @param end End frame
         * @note It is up to you to make sure start is smaller than end.
         */
        public FrameRange(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }

        /// @brief Returns \e i clamped to the range.
        public int Cull(int i)
        {
            if (i < Start)
                i = Start;
            else if (i > End)
                i = End;
            return i;
        }

        /// @brief Returns if \e i is inside [start, end]
        public bool Contains(int i)
        {
            return i >= Start && i <= End;
        }

        /// @brief Returns if the ranges intersect, i.e. touching returns false
        /// @note Assumes They are both valid
        public bool Collides(FrameRange range)
        {
            return Start < range.End && End > range.Start;
            //			return (range.start > start && range.start < end) || (range.end > start && range.end < end );
        }

        /// @brief Returns if the ranges overlap, i.e. touching return true
        /// @note Assumes They are both valid
        public bool Overlaps(FrameRange range)
        {
            return range.End >= Start && range.Start <= End;
        }

        /// @brief Returns what kind of overlap it has with \e range.
        /// @note Assumes They are both valid
        public FrameRangeOverlap GetOverlap(FrameRange range)
        {
            if (range.Start >= Start)
            {
                // contains, left or none
                if (range.End <= End)
                {
                    return FrameRangeOverlap.ContainsFull;
                }
                else
                {
                    if (range.Start > End)
                    {
                        return FrameRangeOverlap.MissOnRight;
                    }
                    return FrameRangeOverlap.ContainsStart;
                }
            }
            else
            {
                // contained, right or none
                if (range.End < Start)
                {
                    return FrameRangeOverlap.MissOnLeft;
                }
                else
                {
                    if (range.End > End)
                    {
                        return FrameRangeOverlap.IsContained;
                    }

                    return FrameRangeOverlap.ContainsEnd;
                }
            }
        }

        public static bool operator ==(FrameRange a, FrameRange b)
        {
            return a.Start == b.Start && a.End == b.End;
        }

        public static bool operator !=(FrameRange a, FrameRange b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;

            return (FrameRange)obj == this;
        }

        public override int GetHashCode()
        {
            return Start + End;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Start, End);
        }
        public static FrameRange Resize(FrameRange cur, FrameRange clampV)
        {
            cur.Start = Mathf.Clamp(cur.Start, clampV.Start, clampV.End);
            cur.End = Mathf.Clamp(cur.End, cur.Start, clampV.End);
            return cur;
        }
    }

    /// @brief Types of range overlap
    public enum FrameRangeOverlap
    {
        MissOnLeft = -2,	/// @brief missed and is to the left of the range passed
        MissOnRight,		/// @brief missed and is to the right of the range passed
        IsContained,		/// @brief overlaps and is contained by the range passed
        ContainsFull,		/// @brief overlaps and contains the range passed
        ContainsStart,		/// @brief overlaps and contains the start of the range passed
        ContainsEnd			/// @brief overlaps and contains the end of the range passed
    }

}