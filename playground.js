import { remote } from 'webdriverio'
const browser = await remote({ capabilities: { browserName: 'chrome' } })
await browser.url('file:///home/mmangel/Workspaces/Github/fable-hub/Fable.UI/main/index.html')
const el = await browser.$('div=Hello World')

console.log(await el.getText())

await browser.deleteSession()
