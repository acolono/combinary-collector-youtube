using System;
using NUnit.Framework;
using YoutubeCollector.Lib;

namespace YoutubeCollector.Tests {
    public class VideoIdParserTests {

        private VideoUrlParser Parser => new VideoUrlParser();

        [Test]
        public void IsId() {
            var id = Parser.GetVideoId("RkEXGgdqMz8");
            Assert.AreEqual("RkEXGgdqMz8", id);
        }

        [Test]
        public void ShortLink() {
            var id = Parser.GetVideoId("https://youtu.be/Jj0ZkflWXQk");
            Assert.AreEqual("Jj0ZkflWXQk", id);
        }

        [Test]
        public void WatchLink() {
            var id = Parser.GetVideoId("https://www.youtube.com/watch?v=Jj0ZkflWXQk");
            Assert.AreEqual("Jj0ZkflWXQk", id);
        }

        [Test]
        public void EmbedLink() {
            var id = Parser.GetVideoId("https://www.youtube.com/embed/RkEXGgdqMz8");
            Assert.AreEqual("RkEXGgdqMz8", id);
        }

        [Test]
        public void AsParameter() {
            var id = Parser.GetVideoId("https://www.youtube.com/watch?index=2&v=c0U4AUTbDC8&t=0s&list=LL2m1XOEtkVR_KGKrZDao20Q");
            Assert.AreEqual("c0U4AUTbDC8", id);
        }

        [Test]
        public void TryAndFail() {
            var b = Parser.TryGetVideoId("https://vimeo.com/98450454", out _);
            Assert.IsFalse(b);
        }

        [Test]
        public void TryAndSucceed() {
            var b = Parser.TryGetVideoId("https://www.youtube.com/embed/XQVrQgOQfaE/crap", out var id);
            Assert.IsTrue(b);
            Assert.AreEqual("XQVrQgOQfaE", id);
        }

        [Test]
        public void Throw() {
            Assert.Throws<Exception>(() => Parser.GetVideoId("https://www.youtube.com/watch?v=💩"));
            Assert.Throws<Exception>(() => Parser.GetVideoId("embed/💩"));
            Assert.Throws<Exception>(() => Parser.GetVideoId(null));
        }
    }
}
