#オレオレ証明書について

すごく脆弱なオレオレ証明書を配置する。

なんと 1 bit の鍵というデフォルトの 512 bit よりもはるかに脆弱な鍵を作成し、有効期限を 7 日とした。

```bash
openssl genrsa 1024 > private-key.pem
openssl req -new -key private-key.pem > my-request.csr
openssl x509 -req -in my-request.csr -signkey private-key.pem -out public-key.crt -days 7
```

## 参考

* [オレオレ証明書をopensslで作る（詳細版）](https://ozuma.hatenablog.jp/entry/20130511/1368284304)
* [RSA鍵、証明書のファイルフォーマットについて](https://qiita.com/kunichiko/items/12cbccaadcbf41c72735)
