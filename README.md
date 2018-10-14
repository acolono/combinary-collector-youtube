[![Container](https://quay.io/repository/0xff/combinary-collector-youtube/status "Container")](https://quay.io/repository/0xff/combinary-collector-youtube)
[![Build Status](https://dev.azure.com/volatile-void/pipes/_apis/build/status/piccaso.combinary-collector-youtube)](https://dev.azure.com/volatile-void/pipes/_build/latest?definitionId=4)

## Example usage:

```yml
version: "2.3"

services:
  collector:
    image: quay.io/0xff/combinary-collector-youtube
    environment:
      # A comma separated list of API keys to use (required)
      - API_KEYS=<API_KEYS>
      # Hostname of postgres instance (required)
      - PG_HOST=db
      # A comma separated list of Channel ID's
      - CHANNEL_IDS=<CHANNEL_IDS>
      # A comma separated list of Playlist ID's
      - LIST_IDS=<LIST_IDS>
      # Specify which collectors are active.
      - COLLECT_VIDEOS=true
      - COLLECT_COMMENTS=false
      - COLLECT_ANSWERS=false
      
  db:
    image: postgres:10-alpine
    expose:
      - "5432"
    volumes:
      - dbdata:/var/lib/postgresql/data

volumes:
  dbdata:

```

There is also an [advanced example](./samples/advanced)

## Webhooks

Webhooks are supported for collecting video details (like from IFTTT) in the form of:
```
/api/get-video-details/{videoIdOrUrl}
```




