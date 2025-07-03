# ICPLAY-DEV

ICPLAY-DEV is a complete gaming ecosystem built on Internet Computer (ICP) blockchain technology, representing a new paradigm in Web3 game development. This project deeply integrates traditional game development technology with modern blockchain technology, creating a decentralized, secure, and user-friendly gaming platform.

## Table of Contents

- [Project Overview](#project-overview)
  - [Core Features](#core-features)
- [Project Architecture](#project-architecture)
- [Technology Stack](#technology-stack)
  - [Game Engine (Unity)](#game-engine-unity)
  - [Frontend (Vue.js)](#frontend-vuejs)
  - [Backend (Node.js)](#backend-nodejs)
  - [Blockchain (ICP)](#blockchain-icp)
- [Quick Start](#quick-start)
  - [Requirements](#requirements)
  - [Installation Steps](#installation-steps)
  - [Running the Project](#running-the-project)
- [Game Features](#game-features)
- [Payment System](#payment-system)
- [Development Guide](#development-guide)
  - [Smart Contract Development](#smart-contract-development)
  - [Frontend Development](#frontend-development)
  - [Game Development](#game-development)
- [Directory Structure](#directory-structure)
  - [Game Project (game-box_pycr/)](#game-project-game-box_pycr)
  - [Frontend Project (frontend_service/)](#frontend-project-frontend_service)
  - [Backend Project (backend_service/)](#backend-project-backend_service)
- [Smart Contracts](#smart-contracts)
  - [Contract Architecture](#contract-architecture)
  - [Technical Features](#technical-features)
- [Security Features](#security-features)
  - [Authentication and Authorization](#authentication-and-authorization)
  - [Encryption and Data Protection](#encryption-and-data-protection)
  - [Payment Security](#payment-security)
  - [Smart Contract Security](#smart-contract-security)
- [Performance Optimization](#performance-optimization)
  - [Unity WebGL Optimization](#unity-webgl-optimization)
  - [Frontend Performance Optimization](#frontend-performance-optimization)
  - [Smart Contract Performance](#smart-contract-performance)
  - [Caching Strategy](#caching-strategy)
  - [Network Optimization](#network-optimization)
  - [Monitoring and Analytics](#monitoring-and-analytics)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgments](#acknowledgments)

## Project Overview

ICPLAY-DEV is a full-stack gaming platform that integrates:
- **Unity Game Engine** - Provides immersive gaming experience
- **Vue.js Frontend** - Modern web user interface
- **Node.js Backend Service** - Handles business logic and payments
- **ICP Smart Contracts** - Decentralized storage and business logic on blockchain
- **Payment System** - ICP token payment support

### Core Features

**Decentralized Architecture**: The project uses ICP blockchain as infrastructure, with all game data, user assets, and transaction records stored in decentralized smart contracts, ensuring data immutability and transparency. Users completely own their gaming assets without relying on centralized servers.

**Multi-Technology Stack Integration**: The frontend uses Vue 3 + Vite to build modern user interfaces, the backend uses Node.js to provide high-performance API services, and the game client is developed based on Unity engine with WebGL deployment support, achieving cross-platform gaming experience.

**Secure Payment System**: Integrates ICP native token payment functionality, supporting multiple tokens including ICP and CKBTC, implementing secure, fast, and low-cost in-game transactions. All payment operations are executed through smart contracts, ensuring transaction security and traceability.

**Scalable Design**: The project adopts a modular architecture with smart contracts divided into platform contracts, user contracts, and indexer contracts, each responsible for different business logic, facilitating future feature expansion and maintenance.

**User Experience Optimization**: Achieves high-performance web gaming experience through Unity WebGL technology, combined with Vue.js responsive design, providing users with smooth game interfaces and interactive experiences. Also supports mobile device adaptation to meet the needs of users on different devices.

**Developer Friendly**: Provides complete development documentation and API interfaces, supports third-party developer integration and extension, building an open ecosystem.

## Project Architecture

```
icplay-dev/
├── game-box_pycr/          # Unity game project
├── frontend_service/       # Vue.js frontend application
├── backend_service/        # Node.js backend service
├── canister_platform/     # ICP platform smart contract
├── canister_user/         # ICP user management smart contract
├── canister_indexer/      # ICP indexer smart contract
├── data-box_pycr/          # Data processing service
└── dfx.json               # ICP deployment configuration
```

## Technology Stack

### Game Engine (Unity)
- **Unity 2022.3+** - Game engine
- **C#** - Programming language
- **DOTween** - Animation system
- **Spine** - 2D skeletal animation
- **WebGL** - Web game support

### Frontend (Vue.js)
- **Vue 3** - Frontend framework
- **Vite** - Build tool
- **Pinia** - State management
- **Vue Router** - Route management
- **@dfinity/agent** - ICP blockchain interaction
- **Sass** - CSS preprocessor

### Backend (Node.js)
- **Express.js** - Web framework
- **@dfinity/identity** - ICP identity authentication
- **@dfinity/ledger-icp** - ICP ledger interaction
- **Redis** - Cache database
- **CORS** - Cross-origin support

### Blockchain (ICP)
- **Rust** - Smart contract language
- **Candid** - Interface definition language
- **ic-cdk** - ICP development toolkit
- **ic-stable-structures** - Stable storage

## Quick Start

### Requirements

- **Node.js** 18+
- **Rust** 1.70+
- **dfx** (Internet Computer SDK)
- **Unity** 2022.3+ (only needed for game development)

### Installation Steps

1. **Clone the project**
```bash
git clone <repository-url>
cd icplay-dev
```

2. **Install ICP development environment**
```bash
# Install dfx
sh -ci "$(curl -fsSL https://internetcomputer.org/install.sh)"
```

3. **Install frontend dependencies**
```bash
cd frontend_service
npm install
```

4. **Install backend dependencies**
```bash
cd backend_service
npm install
```

5. **Build smart contracts**
```bash
# In project root directory
dfx build
```

### Running the Project

1. **Start ICP local network**
```bash
dfx start --background
```

2. **Deploy smart contracts**
```bash
dfx deploy
```

3. **Start backend service**
```bash
cd backend_service
node src/main.js
```

4. **Start frontend service**
```bash
cd frontend_service
npm run dev
```

5. **Build Unity game**
```bash
# Build WebGL version in Unity
# Copy build files to frontend_service/public/Build/
```

## Game Features

- **PLAY YOUR CARDS RIGHT Game** - Classic card game experience
- **Leaderboard System** - Player rankings and achievements
- **Payment System** - ICP token payment support
- **Audio System** - Complete sound effects and background music
- **Responsive Design** - Adapts to mobile and desktop devices

## Payment System

The project integrates a complete payment solution:
- **ICP Token Payment** - Uses Internet Computer native tokens
- **Payment Records** - Complete transaction history
- **Security Verification** - Multi-layer security verification mechanism

## Development Guide

### Smart Contract Development

Smart contracts are written in Rust and located in the following directories:
- `canister_platform/` - Platform core logic
- `canister_user/` - User management
- `canister_indexer/` - Data indexing

### Frontend Development

Frontend is built with Vue 3 + Vite:
```bash
cd frontend_service
npm run dev      # Development mode
npm run build    # Production build
npm run preview  # Preview build results
```

### Game Development

Unity project is located in the `game-box_pycr/` directory:
- Uses Unity 2022.3+ version
- Supports WebGL build
- Integrates JS-Unity communication

## Directory Structure

### Game Project (game-box_pycr/)
```
Assets/
├── Scripts/           # C# script files
├── GameAssets/        # Game resources
├── Framework/         # Game framework
├── Plugins/          # Third-party plugins
└── Scenes/           # Unity scenes
```

### Frontend Project (frontend_service/)
```
src/
├── api/              # API interfaces
├── canisters/        # ICP contract interaction
├── stores/           # Pinia state management
├── views/            # Vue page components
└── router/           # Route configuration
```

### Backend Project (backend_service/)
```
src/
├── config/           # Configuration files
├── paySdk.js         # Payment SDK
└── dishout.js        # Distribution logic
```

## Smart Contracts

The project builds three core smart contracts on the Internet Computer (ICP) blockchain, developed in Rust, implementing a decentralized gaming ecosystem.

### Contract Architecture

#### 1. Platform Contract (canister_platform)
The platform contract is the core of the entire system, responsible for:
- **User Management** - User registration, authentication, and permission control
- **Game Data Storage** - Game state, leaderboards, achievement system
- **Payment Processing** - ICP and CKBTC token transfers and balance management
- **System Configuration** - Environment configuration, fee settings, contract parameter management

**Core Modules:**
- `cycle_management.rs` - Cycle management, ensuring continuous contract operation
- `access_control.rs` - Access control, implementing fine-grained permission management
- `crypto_utils.rs` - Cryptographic utilities, supporting ECDSA signature verification
- `data_store.rs` - Data storage, using stable memory structures

#### 2. User Contract (canister_user)
The user contract focuses on user-related functionality:
- **User Profiles** - Personal information, game statistics, preference settings
- **Payment Accounts** - Multi-token wallet management (ICP, CKBTC)
- **Game Records** - Game history, achievement records, leaderboard data
- **Privacy Protection** - Encrypted storage and access control for user data

**Core Modules:**
- `payment.rs` - Payment processing, supporting ICP and CKBTC transfers
- `access_guard.rs` - Access guards, preventing unauthorized access
- `data_store.rs` - User data storage and management
- `helper_utils.rs` - Helper utility functions

#### 3. Indexer Contract (canister_indexer)
The indexer contract provides efficient data query services:
- **Data Indexing** - Building efficient indexes for game data
- **Historical Records** - Storing and retrieving historical transaction records
- **Data Archiving** - Automatically archiving old data to save storage space
- **Query Optimization** - Providing fast data query interfaces

**Core Features:**
- Supports real-time indexing of up to 2000 messages
- Historical message storage limit of 20000 messages
- Automatic archiving mechanism with 5000 message threshold
- Archive migration size of 500 messages

### Technical Features

#### Stable Storage
Uses the `ic-stable-structures` library for data persistence:
```rust
// Example: User data storage
use ic_stable_structures::{BTreeMap, DefaultMemoryImpl};

type UserStore = BTreeMap<Principal, UserData, DefaultMemoryImpl>;
```

#### Cycle Management
Smart contracts ensure continuous operation through cycle management:
- Automatic monitoring of cycle balance
- Automatic recharge when cycles are low
- Preventing contract stoppage due to insufficient cycles

#### Cross-Contract Communication
Uses ICP's cross-contract calling mechanism:
```rust
// Example: Calling other contracts
let result: Result<(Response,), _> = call(
    target_canister_id,
    "method_name",
    (arg1, arg2)
).await;
```

## Security Features

The project implements a multi-layered security protection system to ensure user asset and data security.

### Authentication and Authorization

#### 1. Access Control Mechanism
- **Owner Permissions** - Only contract owners can execute critical operations
- **Controller Permissions** - Authorized controllers can execute specific functions
- **Anonymous User Protection** - Prohibits anonymous users from accessing sensitive functions

```rust
// Access control example
pub fn owner_guard() -> Result<(), String> {
    let result = data_store::state::with(|s| s.owner_permission(ic_cdk::caller()));
    result
}
```

#### 2. Authentication
- **ICP Identity System** - Uses ICP's native identity authentication
- **Principal Verification** - Verifies the validity of caller identity
- **Session Management** - Secure session creation and verification

### Encryption and Data Protection

#### 1. Encrypted Communication
- **End-to-End Encryption** - All data transmission is encrypted
- **ECDSA Signatures** - Uses Elliptic Curve Digital Signature Algorithm
- **Message Integrity** - Ensures messages are not tampered with during transmission

```rust
// ECDSA signature example
pub async fn sign_with(
    key_name: &str,
    derivation_path: Vec<Vec<u8>>,
    message_hash: [u8; 32],
) -> Result<Vec<u8>, String> {
    // Signature implementation
}
```

#### 2. Encrypted Data Storage
- **Sensitive Data Encryption** - Encrypted storage of user privacy data
- **Key Management** - Secure key generation and storage
- **Data Masking** - Sensitive information is masked in logs

### Payment Security

#### 1. Replay Attack Prevention
- **Unique Transaction IDs** - Each transaction has a unique identifier
- **Timestamp Verification** - Prevents replay of expired transactions
- **Nonce Mechanism** - Uses incremental nonce to prevent replay

#### 2. Amount Verification
- **Balance Checks** - Verifies account balance before transfer
- **Fee Calculation** - Accurate calculation and verification of fees
- **Overflow Protection** - Prevents numeric overflow attacks

```rust
// Payment security example
pub async fn execute_token_transfer(
    token: &str,
    from: Option<[u8; 32]>,
    to: Vec<u8>,
    amount: u64,
) -> Result<u64, String> {
    // Security verification and transfer logic
}
```

### Smart Contract Security

#### 1. Input Validation
- **Parameter Boundary Checks** - Validates the validity of all input parameters
- **Type Safety** - Uses Rust's type system to ensure type safety
- **Format Validation** - Validates the correctness of data formats

#### 2. State Consistency
- **Atomic Operations** - Ensures atomicity of operations
- **State Rollback** - Automatic state rollback on failure
- **Concurrency Control** - Prevents state inconsistency caused by concurrent operations

#### 3. Upgrade Security
- **Upgrade Permission Control** - Only authorized users can upgrade contracts
- **Data Migration** - Secure user data migration during upgrades
- **Backward Compatibility** - Ensures compatibility after upgrades

## Performance Optimization

The project implements performance optimization at multiple levels to ensure efficient system operation and good user experience.

### Unity WebGL Optimization

#### 1. Code Splitting
- **Modular Loading** - Load game modules on demand
- **Resource Packaging** - Split game resources into multiple packages
- **Lazy Loading** - Delay loading of non-critical resources

#### 2. Resource Compression
- **Texture Compression** - Use appropriate texture compression formats
- **Audio Compression** - Optimize audio file sizes
- **Model Optimization** - Reduce 3D model complexity

#### 3. Memory Management
- **Object Pooling** - Reuse game objects to reduce GC pressure
- **Resource Release** - Release unused resources promptly
- **Memory Monitoring** - Real-time monitoring of memory usage

### Frontend Performance Optimization

#### 1. Vite Build Optimization
- **Fast Hot Reload** - Quick refresh during development
- **Code Splitting** - Automatic code splitting and lazy loading
- **Tree Shaking** - Remove unused code

#### 2. Vue 3 Optimization
- **Composition API** - Better logic reuse and performance
- **Reactivity Optimization** - Precise reactive updates
- **Virtual DOM Optimization** - Efficient DOM update algorithms

#### 3. Caching Strategy
- **Browser Caching** - Leverage browser caching mechanisms
- **Service Worker** - Offline caching and background updates
- **CDN Acceleration** - Use CDN to accelerate resource loading

### Smart Contract Performance

#### 1. Cycle Optimization
- **Efficient Algorithms** - Use optimal algorithm implementations
- **Memory Management** - Reasonable use of stable memory
- **Batch Operations** - Batch processing to reduce call frequency

#### 2. Storage Optimization
- **Data Structure Optimization** - Choose appropriate data structures
- **Index Optimization** - Build indexes for query operations
- **Data Compression** - Compress stored data to reduce space usage

```rust
// Storage optimization example
pub const MAX_MSG_COUNT: u64 = 2000;
pub const MAX_HISTORY_MSG_COUNT: u64 = 20000;
pub const ARCHIVE_MSG_THRESHOLD: usize = 5000;
```

#### 3. Query Optimization
- **Pagination** - Implement efficient pagination mechanisms
- **Query Result Caching** - Cache frequently used query results
- **Asynchronous Processing** - Use asynchronous operations to improve concurrent performance

### Caching Strategy

#### 1. Redis Cache Layer
- **Hot Data Caching** - Cache frequently accessed data
- **Session Caching** - Cache user session information
- **Query Result Caching** - Cache database query results

#### 2. Multi-Level Caching
- **L1 Cache** - Fast cache in memory
- **L2 Cache** - Redis distributed cache
- **L3 Cache** - Database persistent storage

#### 3. Cache Policies
- **LRU Algorithm** - Least Recently Used eviction policy
- **TTL Settings** - Reasonable expiration time settings
- **Cache Warming** - Warm up cache when system starts

### Network Optimization

#### 1. Connection Optimization
- **Connection Pooling** - Reuse database and API connections
- **Keep-Alive** - Maintain long connections to reduce handshake overhead
- **Load Balancing** - Distribute request pressure

#### 2. Data Transmission Optimization
- **Data Compression** - Compress transmitted data to reduce bandwidth
- **Incremental Updates** - Only transmit changed data
- **Batch Transmission** - Batch processing to reduce network requests

#### 3. CDN Acceleration
- **Static Resource CDN** - Accelerate static resource loading
- **API CDN** - Accelerate API request responses
- **Global Nodes** - Nearby access to reduce latency

### Monitoring and Analytics

#### 1. Performance Monitoring
- **Response Time Monitoring** - Monitor API response times
- **Throughput Monitoring** - Monitor system processing capacity
- **Error Rate Monitoring** - Monitor system error rates

#### 2. Resource Monitoring
- **CPU Usage** - Monitor CPU usage
- **Memory Usage** - Monitor memory usage
- **Network Bandwidth** - Monitor network bandwidth usage

#### 3. User Behavior Analysis
- **User Path Analysis** - Analyze user operation paths
- **Performance Bottleneck Identification** - Identify system performance bottlenecks
- **Optimization Recommendations** - Provide optimization suggestions based on data

## Contributing

1. Fork the project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Acknowledgments

Thanks to the following open source projects:
- [Internet Computer](https://internetcomputer.org/)
- [Vue.js](https://vuejs.org/)
- [Unity](https://unity.com/)
- [Rust](https://www.rust-lang.org/)
