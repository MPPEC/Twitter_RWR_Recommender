using System;
using System.Collections;
using System.Collections.Generic;

namespace EgoNetworkAnalysis
{
    public class Tweet
    {
        /***************************** Properties *********************************/
        public long ID;
        public long author;
        private bool isMention;

        /***************************** Constructor *********************************/
        public Tweet(long newID, long newAuthor, bool newIsMention)
        {
            this.ID = newID;
            this.author = newAuthor;
            this.isMention = newIsMention;
        }
        public Tweet(Tweet newTweet)
        {
            this.ID = newTweet.ID;
            this.author = newTweet.author;
            this.isMention = newTweet.isMention;
        }

        /***************************** Primary Methods *********************************/
        public bool isMentionTweet() { return isMention; }
        
        /***************************** Override *********************************/
        public override bool Equals(Object obj)
        {
            if (obj is Tweet)
            {
                Tweet otherTweet = (Tweet)obj;
                return (this.ID == otherTweet.ID);
            }
            else
                throw new ArgumentException("Object is not Tweet");
        }
        public override int GetHashCode() { return (int)((this.ID >> 32) ^ this.ID); }
    }
}
