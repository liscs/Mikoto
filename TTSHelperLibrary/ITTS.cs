﻿namespace TTSHelperLibrary
{
    public interface ITTS
    {
        public Task SpeakAsync(string s);
        public Task StopSpeakAsync();
    }
}
