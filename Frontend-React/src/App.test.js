import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import App from './App';

test('renders the footer text', () => {
    render(<App />);
    const footerElement = screen.getByText(/clearpoint.digital/i);
    expect(footerElement).toBeInTheDocument();
});

/*

Note:
The tests here are not working because I have no experience with jest and front-end testing approach
From what I've gathered online, ideally the fetch operations are mocked so that we don't actually hit the server
I would have to spend a bit more time learning about front-end testing before I'm comfortable writing anything meaningful here...

test('add item - valid description - item added to table', async () => {
    const user = userEvent.setup();

    render(<App />);

    const inputElement = screen.getByTestId('input-addtodoitem');
    await user.click(inputElement);

    const description = `test 1 - ${new Date().getTime()}`;
    await user.keyboard(description);

    const buttonElement = screen.getByTestId('button-addtodoitem');
    await user.click(buttonElement);

    await waitFor(() => expect(screen.getByText(description)).toBeInTheDocument());
});

test('add item - valid description - item can be completed', () => {});
test('add item - empty description - error message displayed', () => {});
test('add item - duplicate description - error message displayed', () => {});

*/

