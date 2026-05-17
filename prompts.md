# Session Prompts

1. You are a senior software architect. Review the spec (@spec.md) I have created for a lightweight full-stack reviewer portal using .NET 8 Web API and Vue3. For each issue found, describe the problem and risk, and suggest a fix. Do not update the .md file. I will decide what to change.

2. Format @spec.md file to ensure better readability.

3. You are a senior software engineer who will be implementing the requirements specified on the @spec.md file. Create an implementation-plan.md file which will contain the approach and steps on how you plan to implement the application. The implementation must be done layer by layer. List anything the spec does not clearly define that could block implementation or cause ambiguity.

4. I've reviewed the implementation plan and approve of it. The source of truth is @spec.md and the implementation plan to be followed is @implementation-plan.md. Start implementation of Phase 1.

5. Continue with Phase 2. Do not start Phase 3 implementation.

6. Continue with Phase 3. Do not start Phase 4 implementation.

7. Continue with Phase 4. Do not start Phase 5 implementation.

8. Continue with Phase 5. Do not start Phase 6 implementation.

9. Continue with Phase 6. Do not start Phase 7 implementation.

10. Check the updated @implementation-plan.md and implement the added tests. Update unit tests to ensure it follows Arrange-Act-Assert principle.

11. Continue with Phase 7. Do not start Phase 8 implementation.

12. Continue with Phase 8.

13. Generate a README.md. It must contain these sections:
    1. Project overview (2-3 sentences)
    2. Prerequisites
    3. Running with Docker
    4. Running locally
    5. API endpoints (with request/response examples)
    6. Architecture (under 150 words)

    Ensure to use values that are specified in the files (such as port numbers, etc.)

14. Apply these changes in the web api project:
    - Add an IUniversityApiClient with UniversityApiClient in the Infrastructure layer to separate implementation of fetching from external API. 
    Application layer imports Infrastructure (UniversitySuggestion, IHttpClientFactory)
    - Using Repository pattern does not add value to this project. Remove them and inject AppDbContext into services directly instead.

15. Review the code, analyze the flow, project structure and implemented logic. Evaluate the code objectively and follow industry standard coding practices, Clean Architecture, SOLID principle, RESTful API design principles and maintainability best practices. I want to ensure that it is not over-engineered and that the code is optimized. Provide suggestions on what can be improved on. Do not adapt your feedback based on the existing implementation style.

16. Resolve issues 3, 5-8, 10-12  by applying suggested changes.

17. Update implementation-plan based on previous changes. Minimize code snippets and only add what are necessary for the implementation-plan.

18. Fetch this design file, read its readme, and implement the relevant aspects of the design. Implement: Update the existing code to implement the design based on this design file.

19. No, stick to the original message returned for invite reviewer. No need to return eligibility data.

20. Continue. But do not add the progress chain/steps. That is not necessary.

21. Cleanup styling. Do not use inline styling. Remove unused styles and use global styles for common styles shared across components. Put component-specific styles in the component scoped styles. Leave shared styles in App.vue.

22. Update README.md based on the updated changes, only update the changes that are necessary in the documentation. Then, update instructions on running it on Docker and locally to update config settings for the university api base url.

---

## AI Model Used

**Claude Sonnet 4.6** (via Claude Code CLI) was used throughout this project.

Claude Sonnet 4.6 was chosen for its strong performance on multi-step software engineering tasks, from architecture review, layered implementation, code refactoring, and frontend development, while remaining cost-efficient for an extended session. Its ability to reason across the full stack (C#, Vue 3, Docker, CSS) and retain context across phases made it well-suited for driving a structured, phase-by-phase build of this application.

---

## Notes

AI did not get stuck at any point. After implementation, I requested a thorough code review which surfaced minor improvements, such as data annotation gaps, magic numbers/strings, and naming inconsistencies. These were valid, low-risk fixes that I applied directly.
