<h1 align="center">Bitwise Wizards</h1>
<h4 align="center">"To run, or not to run"</h4>
<p align="center">
    <img src="team/Logo.png" alt="Bitwize Wizards Logo" width="200">
</p>

# About us
We are a committed group of students working combine our skills to address pressing challenges.

# Team members
- Justin
- Rowan
- Jesus
- Paul

# Vision statement
"Our vision is to create meaningful and user-centric solutions that simplify complex problems."

# TrustTrade: Social Trading Platform
## Comprehensive Capstone Project Description

### Project Overview
TrustTrade is a sophisticated social trading platform that bridges the gap between social media and financial technology. The platform enables traders and investors to share verified investment strategies, connect their actual brokerage accounts for transparency, and build a trusted community around authentic financial discussions. This capstone project demonstrates advanced full-stack development skills, complex system architecture, and integration with multiple third-party financial APIs.

**Live Demo:** https://trusttrademedia.azurewebsites.net/ or here which is our most recent migration to railway and postgres: https://trusttrade.up.railway.app/
**Technology Stack:** ASP.NET Core MVC, C#, Entity Framework, SQL Server, SignalR, JavaScript, Bootstrap, Azure

---

## Core Business Problem Solved
Traditional social media platforms lack transparency in financial discussions, making it difficult to distinguish between genuine investment advice and misleading claims. TrustTrade solves this by:
- **Verification System**: Users connect real brokerage accounts through Plaid API integration
- **Transparent Portfolio Tracking**: Real-time portfolio values displayed with posts
- **Community Trust Building**: Performance scoring based on actual trading results
- **Educational Platform**: Verified traders share strategies with provable track records

---

## Technical Architecture

### Backend Infrastructure
**Framework:** ASP.NET Core 8.0 MVC with Entity Framework Core
- **Database:** SQL Server with complex relational data model (20+ entities)
- **Authentication:** ASP.NET Identity with custom user management
- **Real-time Communication:** SignalR for live notifications and messaging
- **Background Services:** Automated data synchronization and news updates
- **API Design:** RESTful API controllers with comprehensive error handling

### Database Design Excellence
**Entities & Relationships:**
- **User Management**: Users, Followers, UserBlocks, VerificationHistory
- **Social Features**: Posts, Comments, Likes, SavedPosts, Messages, Conversations
- **Financial Data**: PlaidConnections, InvestmentPositions, PortfolioVisibilitySettings
- **Content Management**: Photos, Tags, Reports, Notifications
- **Market Data**: Stocks, StockHistory, FinancialNewsItems, FinancialNewsTickerSentiments

**Advanced Database Features:**
- Complex many-to-many relationships with junction tables
- Audit trails for verification status changes
- Optimized queries with lazy loading and query splitting
- Database-first approach with comprehensive constraints and indexes

### External API Integrations

#### Plaid Financial Services API (SANDBOX MODE ONLY)
**Purpose:** Secure brokerage account connectivity and verification
**Implementation:**
- Real-time portfolio data synchronization
- Investment position tracking across multiple institutions
- Secure token exchange and credential management
- Error handling for various financial institutions

#### Alpha Vantage Financial Data API
**Purpose:** Real-time market news and sentiment analysis
**Implementation:**
- Automated news fetching with background services
- Sentiment analysis integration for stocks and cryptocurrencies
- Intelligent categorization and duplicate prevention
- Rate limiting and API quota management

### Advanced Frontend Implementation

#### Real-time Features with SignalR
**Chat System:**
- Instant messaging between users
- Real-time message status updates (sent/read)
- Connection management and reconnection handling
- Group-based conversation management

**Live Notifications:**
- Real-time notification delivery
- Unread count updates
- Interactive notification dropdown with AJAX loading
- Cross-browser compatibility

#### Dynamic User Interface
**Technologies:** Vanilla JavaScript, Bootstrap 5, Chart.js, Cropper.js
- **Responsive Design**: Mobile-first approach with progressive enhancement
- **Interactive Components**: Modal dialogs, infinite scroll, live search
- **Data Visualization**: Portfolio performance charts and market trend graphs
- **Image Processing**: Profile picture cropping and background image management

### Security & Privacy Implementation

#### Data Protection
- **Encryption**: Sensitive financial data encrypted at rest
- **API Security**: Rate limiting and request validation
- **User Privacy**: Granular portfolio visibility controls
- **Content Moderation**: Comprehensive reporting and admin management system

#### User Verification System
**Multi-level verification process:**
1. **Account Creation**: Standard email/password authentication
2. **Brokerage Connection**: Plaid API integration for account verification
3. **Portfolio Tracking**: Real-time investment position monitoring
4. **Community Trust**: Performance scoring based on actual trading results

---

## Key Features & Functionality

### 1. User Management & Social Features
**Profile System:**
- Customizable user profiles with bio, trading preferences, and profile pictures
- Background image support (file upload or URL-based)
- Performance scoring with detailed breakdowns
- Verification badge system for connected users

**Social Networking:**
- Follow/unfollow system with follower/following lists
- User blocking and content reporting
- Private messaging with real-time delivery
- User search and discovery

### 2. Content Management System
**Post Creation & Management:**
- Rich text posts with image upload support
- Tag system for content categorization
- Public/private post visibility controls
- Edit and delete functionality with ownership validation

**Engagement Features:**
- Like/unlike system for posts and comments
- Hierarchical comment system with nested replies
- Save/unsave posts for later reference
- Real-time engagement metrics

### 3. Financial Integration
**Portfolio Connectivity:**
- Secure Plaid integration for 10,000+ financial institutions(sandbox only)
- Real-time portfolio value display with posts
- Investment position tracking and visualization
- Historical performance analysis

**Market Data:**
- Live stock and cryptocurrency prices
- Interactive charts with multiple timeframe options
- Market news feed with sentiment analysis
- Performance scoring algorithm based on portfolio performance

### 4. Administrative Features
**Content Moderation:**
- Admin panel for user management
- Post and comment moderation tools
- User suspension and reactivation
- Comprehensive reporting system

**System Management:**
- Presentation mode for live demonstrations
- Site-wide settings management
- User verification history tracking
- Performance analytics and monitoring

### 5. Real-time Communication
**Chat System:**
- One-on-one messaging between users
- Message read receipts and delivery status
- Conversation history and search
- Mobile-responsive chat interface

**Notification System:**
- Real-time notification delivery
- Customizable notification preferences
- Notification history and archiving
- Email notification integration

---

## Performance & Optimization

### Database Optimization
- **Query Optimization**: Implemented lazy loading with strategic eager loading for performance-critical paths
- **Indexing Strategy**: Created indexes on frequently queried columns (user searches, post filtering)
- **Query Splitting**: Used EF Core query splitting for complex many-to-many relationships

### Frontend Performance
- **Lazy Loading**: Implemented infinite scroll for post feeds and pagination for large datasets
- **Asset Optimization**: Minified CSS/JavaScript and optimized image loading
- **Caching Strategy**: Browser caching for static assets and API response caching

### API Management
- **Rate Limiting**: Implemented throttling for external API calls
- **Error Handling**: Comprehensive error handling with user-friendly error messages
- **Retry Logic**: Automatic retry mechanisms for failed external API calls

---

## Development Methodology

### Agile Practices
- **Sprint Planning**: Bi-Weekly sprints with defined user stories and acceptance criteria
- **Daily Standups**: Regular team synchronization and blocker identification
- **Retrospectives**: Continuous improvement through team feedback sessions and professor
- **Version Control**: Git-based workflow with feature branches and code reviews

### Project Management
- **Jira Integration**: Ticket tracking, sprint management, and team coordination
- **Documentation**: Comprehensive README files, API documentation, and code comments
- **Testing Strategy**: Unit tests for critical business logic and integration tests for API endpoints

### Quality Assurance
- **Code Reviews**: Peer review process for all code changes
- **Error Handling**: Comprehensive exception handling with logging
- **Input Validation**: Client-side and server-side validation for all user inputs

---

## Business Value & Impact

### User Benefits
- **Transparency**: Verified portfolio information builds trust in investment discussions
- **Education**: Learn from successful traders with proven track records
- **Community**: Connect with like-minded investors and share strategies
- **Convenience**: Centralized platform for social networking and portfolio tracking

### Technical Innovation
- **API Integration**: Demonstrates mastery of complex third-party API integration
- **Real-time Features**: Showcases modern web application development with SignalR
- **Security Implementation**: Exhibits understanding of financial data security requirements
- **Scalable Architecture**: Built with enterprise-level scalability considerations

### Professional Development
- **Full-Stack Expertise**: Demonstrates proficiency across entire technology stack
- **Financial Domain Knowledge**: Understanding of fintech requirements and regulations
- **Problem-Solving**: Creative solutions to complex technical and business challenges

---

## Future Enhancements & Roadmap

### Technical Improvements
- **Microservices Architecture**: Decompose monolith into scalable microservices
- **Advanced Caching**: Redis implementation for session management and caching
- **API Versioning**: RESTful API versioning strategy for mobile app support
- **Monitoring & Analytics**: Application performance monitoring and user analytics

---

## Conclusion

TrustTrade represents a sophisticated fusion of social networking and financial technology, demonstrating advanced full-stack development capabilities and deep understanding of modern web application architecture. The project showcases expertise in:

- **Complex System Design**: Multi-layered architecture with separation of concerns
- **API Integration**: Seamless integration with financial services and market data providers  
- **Real-time Features**: Modern web technologies for enhanced user experience
- **Security & Compliance**: Financial data protection and user privacy considerations
- **Scalable Development**: Enterprise-ready codebase with growth potential

This capstone project serves as a comprehensive demonstration of software engineering skills, business acumen, and technical innovation suitable for enterprise-level application development.
