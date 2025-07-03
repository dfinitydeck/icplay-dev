# Gamebox API Documentation

## Login

```ts
window.gamebox.login():Promise<string>
```

### Returns

- Principal after login

## Logout

```ts
window.gamebox.logout():Promise<void>
```

## Get Balance

```ts
window.gamebox.getBalance(principal?:string):Promise<number>
```

### Parameters

- principal: Any principal, if not provided, uses the current logged-in user's principal

### Returns

- Balance, unit: ICP

## Purchase Package

```ts
window.gamebox.buy(packId:string):Promise<{
  code: number // 0: success, -1: failure
  msg: string // error message
}>
```

### Parameters

- packId: Package ID

### Returns

- Whether the transfer was successful

## Get Current Logged-in User's Principal

```ts
window.gamebox.getPrincipal():string
```

### Returns

- Current logged-in user's principal
